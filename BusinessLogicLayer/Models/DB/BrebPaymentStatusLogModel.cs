using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models.DB
{
    public class BrebPaymentStatusLogModel
    {
        public long ReqId { get; set; }
        public string? RefNo { get; set; }
        public DateTime? ReqTime { get; set; }
        public string? ClientReq { get; set; }
        public string? Method { get; set; }
        public string? RespData { get; set; }
        public DateTime? RespTime { get; set; }
    }
}
