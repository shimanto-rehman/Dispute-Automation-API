namespace RemitAnnex.Models.DB
{
    public class SaveErrorLog
    {
        public int serviceId { get; set; }
        public int logId { get; set; }
        public string searchKey { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string stackTrace { get; set; }
    }
}
