namespace BusinessLogicLayer.Models.Response.BREB
{
    public class BrebDisputeResponse
    {
        public int Status { get; set; }
        public BrebDisputeData? Data { get; set; }
        public List<BrebDisputeError>? Errors { get; set; }
    }

    public class BrebDisputeData
    {
        public string? SMSAccountNo { get; set; }
        public string? RefNo { get; set; }
        public string? PayerTransactionId { get; set; }
        public int? DisputeType { get; set; } // 5 or 10
        public string? Message { get; set; }
    }
    public class BrebDisputeError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}