using BusinessLogicLayer.Enums;

namespace BusinessLogicLayer.Models.Request
{
    public class ProcessCollectionRequest
    {
        public string ClientType { get; set; }
        public required int ClientId { get; set; }
        public string BillerId { get; set; }
        public int IssueId { get; set; }
    }
}