using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models.DB
{
    public class CollectionModel
    {
        public int CollectionId { get; set; }
        public byte? CollStatusId { get; set; }
        public int? ApproveBy { get; set; }
        public string? Remarks { get; set; }
        public int? ClientId { get; set; }
        public string? BranchCode { get; set; }

    }

}
