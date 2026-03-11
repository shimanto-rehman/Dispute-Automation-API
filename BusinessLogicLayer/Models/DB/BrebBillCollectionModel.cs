using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models.DB
{
    public class BrebBillCollectionModel
    {
        public long AutoId { get; set; }

        public long CollectionId { get; set; }

        public string ? RefNo { get; set; }

        public string? TransactionId { get; set; }

        public string? SMSAccountNo { get; set; }

        public string? BranchRoutingNo { get; set; }

        public string? DueType { get; set; }

        public string? BillMonth { get; set; }

        public string? MobileNo { get; set; }

        public decimal? BillAmount { get; set; }

        public decimal? VatAmount { get; set; }

        public decimal? LPC { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? CollectionDate { get; set; }

        public DateTime? PaymentDateTime { get; set; }

        public string? Remarks { get; set; }
    }
}
