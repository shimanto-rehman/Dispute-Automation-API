using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models.Response.BREB
{
    public class BrebAutomationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IssueMatchInfo IssueMatch { get; set; }
        public PaymentStatusInfo PaymentStatus { get; set; }
        public DisputeInfo Dispute { get; set; }
        public CollectionUpdateInfo CollectionUpdate { get; set; }
    }
    public class RefNoSourceInfo
    {
        public long ReqId { get; set; }
        public string ResolvedRefNo { get; set; }
        public DateTime? ReqTime { get; set; }
        public DateTime? RespTime { get; set; }
        public string TransactionId { get; set; }
    }
    public class IssueMatchInfo
    {
        public int RequestedIssueId { get; set; }
        public int? ActualIssueId { get; set; }
        public bool IsMatched { get; set; }
        public string ActualStatusDescription { get; set; }
        public string ActualBrebStatus { get; set; }
    }

    public class PaymentStatusInfo
    {
        public string ResponseCode { get; set; }
        public string SMSAccountNo { get; set; }
        public string RefNo { get; set; }
        public string PayerTransactionId { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public string Message { get; set; }
    }

    public class DisputeInfo
    {
        public bool Attempted { get; set; }
        public int? DisputeTypeSent { get; set; }
        public string DisputeTypeDescription { get; set; }
        public bool ApiCallSucceeded { get; set; }
        public int? BrebDisputeStatus { get; set; }
        public string DisputeMessage { get; set; }
        public List<string> DisputeErrors { get; set; }
    }

    public class CollectionUpdateInfo
    {
        public bool Attempted { get; set; }
        public bool IsSuccessful { get; set; }
        public int CollectionId { get; set; }
        public string Message { get; set; }
    }
}