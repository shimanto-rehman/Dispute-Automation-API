namespace BusinessLogicLayer.Models.Request.BREB
{
    public class BrebPaymentStatusRequest
    {
        public string SMSAccountNo { get; set; }
        public string RefNo { get; set; }
        public string PayerTransactionId { get; set; }
        public string ChannelId { get; set; }
    }
}