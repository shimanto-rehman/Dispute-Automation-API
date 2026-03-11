using System;

namespace BusinessLogicLayer.Models.Response.BREB
{
    public class BrebPaymentStatusResponse
    {
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public BrebPaymentStatusResult? Result { get; set; }
        public List<BrebErrorResult>? Errors { get; set; }
    }

    public class BrebPaymentStatusResult
    {
        public string? SMSAccountNo { get; set; }
        public string? RefNo { get; set; }
        public DateTime? PaymentTimestamp { get; set; }
        public string? DueType { get; set; }
        public string? BillMonth { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Vat { get; set; }
        public int? ExpiresIn { get; set; }
        public string? PayerTransactionId { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
    }
    public class BrebErrorResult
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}