using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Manager.BREB;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Request.BREB;
using BusinessLogicLayer.Models.Response.BREB;
using BusinessLogicLayer.ServiceInvoker;
using DataAccessLayer.ServiceInvoker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IClientManager _clientManager;
        private readonly IBrebManager _brebManager;
        private readonly IBrebService _brebService;
        private readonly ILogger<UtilityController> _logger;
        public UtilityController(IClientManager clientManager, IBrebManager brebManager, IBrebService brebService, ILogger<UtilityController> logger)
        {
            this._clientManager = clientManager;
            this._brebManager = brebManager;
            this._brebService = brebService;
            _logger = logger;
        }
        [HttpPost("breb")]
        public async Task<IActionResult> BREB([FromBody] ProcessCollectionRequest collectionRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that the requested IssueId is a known value (1, 2, or 3)
            var expectedBrebStatus = BrebIssueResolver.GetStatusFromIssueId(collectionRequest.IssueId);
            if (expectedBrebStatus == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Invalid IssueId '{collectionRequest.IssueId}'. " +
                              "Valid values: 1 = Paid Acknowledged (status 50), " +
                              "2 = Waiting for Acknowledge (status 30), " +
                              "3 = Pending (status 10)."
                });
            }

            // ── Step 1: Resolve the internal collection record ─────────────────────────
            CollectionModel? collectionModel =
                await _clientManager.ProcessCollectionAsync(collectionRequest);

            if (collectionModel == null)
            {
                _logger.LogWarning("ProcessCollectionAsync returned null for ClientId={Id}", collectionRequest.ClientId);
                return Ok(new BrebAutomationResponse
                {
                    Success = false,
                    Message = "No collection record found for the given request."
                });
            }

            // ── Step 2: Load BREB bill collection details ──────────────────────────────
            BrebBillCollectionModel brebBillCollectionModel =
                await _brebManager.getBrebBillCollectionByCollectionId(collectionModel.CollectionId);

            if (brebBillCollectionModel == null)
            {
                _logger.LogWarning("No BREB bill collection found for CollectionId={CollectionId}", collectionModel.CollectionId);
                return Ok(new BrebAutomationResponse
                {
                    Success = false,
                    Message = $"No BREB bill collection record found for CollectionId={collectionModel.CollectionId}."
                });
            }

            if (string.IsNullOrWhiteSpace(brebBillCollectionModel.TransactionId))
            {
                _logger.LogWarning(
                    "TransactionId is null or empty | CollectionId={Id}",
                    collectionModel.CollectionId);

                return Ok(new BrebAutomationResponse
                {
                    Success = false,
                    Message = $"TransactionId is missing for CollectionId={collectionModel.CollectionId}. " +
                              "Cannot resolve RefNo from the payment submit log."
                });
            }

            BrebPaymentStatusLogModel? submitLog =
                await _brebManager.getPaymentStatusRefNoAsync(brebBillCollectionModel.TransactionId);

            if (submitLog == null || string.IsNullOrWhiteSpace(submitLog.RefNo))
            {
                _logger.LogWarning(
                    "PaymentSubmit log not found or ref_id empty | TransactionId={TxId} | CollectionId={Id}",
                    brebBillCollectionModel.TransactionId, collectionModel.CollectionId);

                return Ok(new BrebAutomationResponse
                {
                    Success = false,
                    Message = $"No PaymentSubmit log found for TransactionId='{brebBillCollectionModel.TransactionId}' " +
                              $"today. RefNo cannot be resolved for CollectionId={collectionModel.CollectionId}."
                });
            }

            string resolvedRefNo = submitLog.RefNo;

            _logger.LogInformation(
                "RefNo resolved | TransactionId={TxId} | RefNo={RefNo} | ReqId={ReqId}",
                brebBillCollectionModel.TransactionId, resolvedRefNo, submitLog.ReqId);

            // ── Step 3: Query BREB payment status ──────────────────────────────────────
            var paymentStatusRequest = new BrebPaymentStatusRequest
            {
                SMSAccountNo = brebBillCollectionModel.SMSAccountNo,
                ChannelId = "UTILITY",
                RefNo = resolvedRefNo,
                PayerTransactionId = brebBillCollectionModel.TransactionId
            };

            BrebPaymentStatusResponse? brebPaymentStatusResponse =
                await _brebService.GetPaymentStatusAsync(paymentStatusRequest);

            // Build the unified response we will fill in progressively
            var automationResponse = new BrebAutomationResponse();

            // BREB API call itself failed (non-2xx HTTP or null)
            if (brebPaymentStatusResponse == null)
            {
                _logger.LogError("GetPaymentStatusAsync returned null for SMSAccountNo={SMSAccountNo}", paymentStatusRequest.SMSAccountNo);
                automationResponse.Success = false;
                automationResponse.Message = "BREB payment status API call failed or returned no response.";
                return StatusCode(502, automationResponse);
            }

            // BREB returned an application-level error (ResponseCode != "000")
            if (brebPaymentStatusResponse.ResponseCode != "000" || brebPaymentStatusResponse.Result == null)
            {
                automationResponse.Success = false;
                automationResponse.Message = "BREB payment status API returned an unsuccessful response.";
                automationResponse.PaymentStatus = new PaymentStatusInfo
                {
                    ResponseCode = brebPaymentStatusResponse.ResponseCode,
                    Message = brebPaymentStatusResponse.Message
                };
                return Ok(automationResponse);
            }

            // ── Step 4: Resolve actual issue and compare with requested IssueId ────────
            string actualBrebStatus = brebPaymentStatusResponse.Result.Status ?? string.Empty;
            int? actualIssueId = BrebIssueResolver.GetIssueIdFromStatus(actualBrebStatus);
            string actualStatusDescription = BrebIssueResolver.GetLabelFromStatus(actualBrebStatus);
            bool issueMatched = actualIssueId.HasValue &&
                                              actualIssueId.Value == collectionRequest.IssueId;

            automationResponse.IssueMatch = new IssueMatchInfo
            {
                RequestedIssueId = collectionRequest.IssueId,
                ActualIssueId = actualIssueId,
                IsMatched = issueMatched,
                ActualBrebStatus = actualBrebStatus,
                ActualStatusDescription = actualStatusDescription
            };

            automationResponse.PaymentStatus = new PaymentStatusInfo
            {
                ResponseCode = brebPaymentStatusResponse.ResponseCode,
                SMSAccountNo = brebPaymentStatusResponse.Result.SMSAccountNo,
                RefNo = brebPaymentStatusResponse.Result.RefNo,
                PayerTransactionId = brebPaymentStatusResponse.Result.PayerTransactionId,
                Status = actualBrebStatus,
                StatusDescription = actualStatusDescription,
                Message = brebPaymentStatusResponse.Result.Message
            };

            // IssueId mismatch → inform caller, do NOT proceed with any DB updates
            if (!issueMatched)
            {
                automationResponse.Success = false;
                automationResponse.Message =
                    $"Issue mismatch: you submitted IssueId={collectionRequest.IssueId} " +
                    $"but the actual BREB payment status is '{actualBrebStatus}' " +
                    $"({actualStatusDescription}, IssueId={actualIssueId?.ToString() ?? "unknown"}). " +
                    "No changes were made.";

                _logger.LogWarning(
                    "IssueId mismatch: requested={Requested}, actual={Actual} ({Label}) for CollectionId={CollId}",
                    collectionRequest.IssueId, actualBrebStatus, actualStatusDescription, collectionModel.CollectionId);

                return Ok(automationResponse);
            }

            // ── Step 5: Execute action based on matched status ────────────────────────

            // --- 5a. Status 50: Paid Acknowledged → update collection directly ----------
            if (actualBrebStatus == "50")
            {
                automationResponse.Dispute = new DisputeInfo { Attempted = false };

                bool updateResult = await _brebManager.updateCollectionTable(collectionModel);

                automationResponse.CollectionUpdate = new CollectionUpdateInfo
                {
                    Attempted = true,
                    IsSuccessful = updateResult,
                    CollectionId = collectionModel.CollectionId,
                    Message = updateResult
                        ? "Collection table updated successfully."
                        : "Collection table update failed. Stored procedure returned 0 rows affected."
                };

                automationResponse.Success = updateResult;
                automationResponse.Message = updateResult
                    ? "Payment confirmed as Paid Acknowledged. Collection record updated successfully."
                    : "Payment is Paid Acknowledged but collection table update failed.";

                _logger.LogInformation(
                    "Status=50 | CollectionId={Id} | UpdateSuccess={Result}",
                    collectionModel.CollectionId, updateResult);

                return Ok(automationResponse);
            }

            // --- 5b. Status 10 or 30 → call dispute API, then conditionally update ------
            if (actualBrebStatus == "10" || actualBrebStatus == "30")
            {
                int? disputeType = BrebIssueResolver.GetDisputeTypeFromStatus(actualBrebStatus);
                string disputeTypeDescription = disputeType == 5 ? "Acknowledge (5)" : "Reset (10)";

                var disputeInfo = new DisputeInfo
                {
                    Attempted = true,
                    DisputeTypeSent = disputeType,
                    DisputeTypeDescription = disputeTypeDescription
                };

                var brebDisputeRequest = new BrebDisputeRequest
                {
                    SMSAccountNo = paymentStatusRequest.SMSAccountNo,
                    ChannelId = paymentStatusRequest.ChannelId,
                    RefNo = paymentStatusRequest.RefNo,
                    PayerTransactionId = paymentStatusRequest.PayerTransactionId,
                    DisputeType = disputeType!.Value  // never null for status 10/30
                };

                BrebDisputeResponse? disputeResponse =
                    await _brebService.UpdateDisputeAsync(brebDisputeRequest);

                if (disputeResponse == null)
                {
                    // HTTP-level failure from dispute API
                    disputeInfo.ApiCallSucceeded = false;
                    disputeInfo.DisputeMessage = "Dispute API call failed or returned no response.";

                    automationResponse.Dispute = disputeInfo;
                    automationResponse.CollectionUpdate = new CollectionUpdateInfo
                    {
                        Attempted = false,
                        IsSuccessful = false,
                        CollectionId = collectionModel.CollectionId,
                        Message = "Skipped: dispute API call failed."
                    };
                    automationResponse.Success = false;
                    automationResponse.Message = "Dispute API call failed. Collection table was not updated.";

                    _logger.LogError(
                        "Dispute API returned null | CollectionId={Id} | DisputeType={DT}",
                        collectionModel.CollectionId, disputeType);

                    return StatusCode(502, automationResponse);
                }

                // Collect dispute errors for response (if any)
                var disputeErrorMessages = disputeResponse.Errors?
                    .Select(e => $"[{e.Code}] {e.Message}")
                    .ToList() ?? new List<string>();

                disputeInfo.ApiCallSucceeded = true;
                disputeInfo.BrebDisputeStatus = disputeResponse.Status;
                disputeInfo.DisputeMessage = disputeResponse.Data?.Message
                                                ?? (disputeErrorMessages.Any()
                                                    ? string.Join("; ", disputeErrorMessages)
                                                    : "No message returned.");
                disputeInfo.DisputeErrors = disputeErrorMessages;

                _logger.LogInformation(
                    "Dispute API response | CollectionId={Id} | BrebStatus={BrebStatus} | DisputeType={DT} | DisputeResponseStatus={DS}",
                    collectionModel.CollectionId, actualBrebStatus, disputeType, disputeResponse.Status);

                // Only update the collection table when dispute API returned 200 + non-null Data
                bool disputeAccepted = disputeResponse.Status == 200 && disputeResponse.Data != null;

                if (disputeAccepted)
                {
                    bool updateResult = await _brebManager.updateCollectionTable(collectionModel);

                    automationResponse.CollectionUpdate = new CollectionUpdateInfo
                    {
                        Attempted = true,
                        IsSuccessful = updateResult,
                        CollectionId = collectionModel.CollectionId,
                        Message = updateResult
                            ? "Collection table updated successfully after dispute."
                            : "Collection table update failed. Stored procedure returned 0 rows affected."
                    };

                    automationResponse.Success = updateResult;
                    automationResponse.Message = updateResult
                        ? $"Dispute accepted ({disputeTypeDescription}). Collection record updated successfully."
                        : $"Dispute accepted ({disputeTypeDescription}) but collection table update failed.";

                    _logger.LogInformation(
                        "UpdateCollection after dispute | CollectionId={Id} | UpdateSuccess={Result}",
                        collectionModel.CollectionId, updateResult);
                }
                else
                {
                    // Dispute returned 202 or unexpected status — do not update collection
                    automationResponse.CollectionUpdate = new CollectionUpdateInfo
                    {
                        Attempted = false,
                        IsSuccessful = false,
                        CollectionId = collectionModel.CollectionId,
                        Message = $"Skipped: dispute API returned status {disputeResponse.Status}. " +
                                       "Manual reconciliation may be required."
                    };

                    automationResponse.Success = false;
                    automationResponse.Message =
                        $"Dispute API returned status {disputeResponse.Status} " +
                        $"({(disputeErrorMessages.Any() ? string.Join("; ", disputeErrorMessages) : "see dispute details")}). " +
                        "Collection table was not updated.";

                    _logger.LogWarning(
                        "Dispute not accepted | CollectionId={Id} | DisputeStatus={DS} | Errors={Errs}",
                        collectionModel.CollectionId, disputeResponse.Status,
                        string.Join("; ", disputeErrorMessages));
                }

                automationResponse.Dispute = disputeInfo;
                return Ok(automationResponse);
            }

            // ── Fallback: unrecognised BREB status (should not be reachable after mapping check) ──
            automationResponse.Success = false;
            automationResponse.Message = $"Unrecognised BREB payment status '{actualBrebStatus}'. No action taken.";
            return Ok(automationResponse);
        }
        private static CollectionUpdateInfo BuildCollectionUpdateInfo(
            int collectionId, bool attempted, bool success, string message)
            => new CollectionUpdateInfo
            {
                Attempted = attempted,
                IsSuccessful = success,
                CollectionId = collectionId,
                Message = message
            };
    }
}
