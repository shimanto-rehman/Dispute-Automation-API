using System.Text.Json;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Manager.BREB;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Request.BREB;
using BusinessLogicLayer.Models.Response;
using BusinessLogicLayer.Models.Response.BREB;
using BusinessLogicLayer.ServiceInvoker;
using DataAccessLayer.Repositories.BREB;
using DataAccessLayer.ServiceInvoker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DisputeAutomation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IClientManager _clientManager;
        private readonly IBrebManager _brebManager;
        private readonly IBrebService _brebService;
        private readonly IDBLogManager _dbLog;
        private readonly MessageSettings _messages;
        private static readonly JsonSerializerOptions _jsonOpts =
            new JsonSerializerOptions { WriteIndented = false };
        public UtilityController(IClientManager clientManager, IBrebManager brebManager, IBrebService brebService, IDBLogManager dbLog, IOptions<MessageSettings> messageOptions)
        {
            this._clientManager = clientManager;
            this._brebManager = brebManager;
            this._brebService = brebService;
            this._messages = messageOptions.Value;
            _dbLog = dbLog;
        }
        [HttpPost("breb")]
        public async Task<IActionResult> BREB([FromBody] ProcessCollectionRequest collectionRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            try
            {
                await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                {
                    ServiceId = collectionRequest.ClientId,
                    RefNo = collectionRequest.BillerId,
                    JsonData = JsonSerializer.Serialize(collectionRequest),
                    TranType = "BREB",
                    DataType = "Request",
                    EndPoint = "UtilityController-BREB"
                });

                CollectionModel? collectionModel = await _clientManager.ProcessCollectionAsync(collectionRequest);

                if (collectionModel == null)
                {
                    var msgBody = _messages.BREB.Error.NoCollectionFound.Msg;
                    var msgCode = _messages.BREB.Error.NoCollectionFound.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = string.Empty });
                }

                BrebBillCollectionModel brebBillCollectionModel = await _brebManager.getBrebBillCollectionByCollectionId(collectionModel.CollectionId, Convert.ToInt32(collectionModel.ClientId));


                if (brebBillCollectionModel == null)
                {
                    var msgBody = string.Format(_messages.BREB.Error.NoBrebCollectionFound.Msg, collectionModel.CollectionId);
                    var msgCode = _messages.BREB.Error.NoBrebCollectionFound.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = string.Empty });
                }

                if (string.IsNullOrWhiteSpace(brebBillCollectionModel.TransactionId))
                {
                    var msgBody = string.Format(_messages.BREB.Error.TransactionIdMissing.Msg, collectionModel.CollectionId);
                    var msgCode = _messages.BREB.Error.TransactionIdMissing.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = string.Empty });
                }

                BrebPaymentStatusLogModel? submitLog = await _brebManager.getPaymentStatusRefNoAsync(brebBillCollectionModel.TransactionId, Convert.ToInt32(collectionModel.ClientId));



                if (submitLog == null || string.IsNullOrWhiteSpace(submitLog.RefNo))
                {
                    var msgBody = string.Format(_messages.BREB.Error.PaymentSubmitLogMissing.Msg, brebBillCollectionModel.TransactionId, collectionModel.CollectionId);
                    var msgCode = _messages.BREB.Error.PaymentSubmitLogMissing.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = string.Empty });
                }

                var paymentStatusRequest = new BrebPaymentStatusRequest
                {
                    SMSAccountNo = brebBillCollectionModel.SMSAccountNo,
                    ChannelId = "UTILITY",
                    RefNo = submitLog.RefNo,
                    PayerTransactionId = brebBillCollectionModel.TransactionId
                };

                BrebPaymentStatusResponse? brebPaymentStatusResponse = await _brebService.GetPaymentStatusAsync(paymentStatusRequest, collectionRequest.ClientId, collectionRequest.BillerId);


                bool updateResult = false;
                if (brebPaymentStatusResponse != null && brebPaymentStatusResponse.ResponseCode == "000")
                {
                    collectionModel.Remarks = "Automation Update";
                    collectionModel.ApproveBy = 1;
                    if (brebPaymentStatusResponse.Result.Status == "50")
                    {
                        updateResult = await _brebManager.updateCollectionTable(collectionModel);

                    }
                    else if (brebPaymentStatusResponse.Result.Status == "30" || brebPaymentStatusResponse.Result.Status == "10")
                    {
                        var brebDisputeRequest = new BrebDisputeRequest
                        {
                            SMSAccountNo = paymentStatusRequest.SMSAccountNo,
                            ChannelId = paymentStatusRequest.ChannelId,
                            RefNo = paymentStatusRequest.RefNo,
                            PayerTransactionId = paymentStatusRequest.PayerTransactionId,
                            DisputeType = collectionModel.CollStatusId == 1 ? 5 : 10
                        };


                        BrebDisputeResponse? disputeResponse = await _brebService.UpdateDisputeAsync(brebDisputeRequest, collectionRequest.ClientId, collectionRequest.BillerId);

                        if (disputeResponse != null && disputeResponse.Status == 200)
                        {
                            updateResult = await _brebManager.updateCollectionTable(collectionModel);
                        }
                    }
                }
                if (updateResult)
                {
                    await _dbLog.SaveReqRespLog(new MwApiReqRespLog
                    {
                        ServiceId = collectionRequest.ClientId,
                        RefNo = collectionRequest.BillerId,
                        JsonData = JsonSerializer.Serialize(collectionModel),
                        TranType = "BREB",
                        DataType = "Response",
                        EndPoint = "UtilityController-BREB"
                    });
                    var msgBody = _messages.BREB.Success.CollectionUpdateSuccess.Msg;
                    var msgCode = _messages.BREB.Success.CollectionUpdateSuccess.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = collectionModel });

                }
                else
                {
                    var msgBody = _messages.BREB.Error.CollectionUpdateFailed.Msg;
                    var msgCode = _messages.BREB.Success.CollectionUpdateSuccess.StatusCode;
                    return Ok(new { Success = false, Message = msgBody, StatusCode = msgCode, Data = collectionModel });
                }
            }
            catch (Exception ex)
            {
                await _dbLog.SaveErrorLog(
                    new SaveErrorLog
                    {
                        ControllerName = nameof(UtilityController),
                        MethodName = nameof(BREB),
                        ServiceId = collectionRequest.ClientId.ToString(),
                        ErrorMsg = ex.Message,
                        StackTrace = ex.StackTrace.ToString()
                    }
                );
            }
            var msg = _messages.BREB.Error.CollectionUpdateFailed.Msg;
            var code = _messages.BREB.Error.CollectionUpdateFailed.StatusCode;
            return Ok(new { Success = false, Message = msg, StatusCode = code, Data = string.Empty });
        }

    }
}
