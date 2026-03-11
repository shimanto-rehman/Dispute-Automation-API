namespace BusinessLogicLayer.Models.Request.BREB
{
    public class BrebDisputeRequest
    {
        public string SMSAccountNo { get; set; }
        public string RefNo { get; set; }
        public string PayerTransactionId { get; set; }
        public int DisputeType { get; set; } // 5 for Acknowledge, 10 for Dispute/Cancel
        public string ChannelId { get; set; } = "UTILITY";
    }
}