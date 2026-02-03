namespace RemitAnnex.Models.Request
{
    public class RemitAnexUpdateRequest
    {
        public List<TranDetails>  TranDetails { get; set; }
        public MakerDetails MakerDetails { get; set; }
    }
    public class TranDetails
    {
        public int PartyId { get; set; }
        public string ReferenceNumber { get; set; }
        public int EntityId { get; set; }
        public string TranChannel { get; set; }
        public bool IsIncentive { get; set; }
        public bool IsReverse { get; set; }
        public string StatusCode { get; set; }
    }

    public class MakerDetails
    {
        public string PurposeOfUpdate { get; set; }
        public string EmployeeId { get; set; }
    }
}
