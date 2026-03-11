using System;

namespace BusinessLogicLayer.Models.DB
{
    public class MwApiReqRespLog
    {
        public int? ServiceId { get; set; }
        public string? RefNo { get; set; }
        public string? JsonData { get; set; }
        public string? TranType { get; set; }
        public string? DataType { get; set; }
        public string? EndPoint { get; set; }
    }
}