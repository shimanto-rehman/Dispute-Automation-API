using System;

namespace BusinessLogicLayer.Models.Response
{
    public class ProcessCollectionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long CollectionId { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string ClientType { get; set; } // BREB, WASA, DESCO, DPDC
    }
}