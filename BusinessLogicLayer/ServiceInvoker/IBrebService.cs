using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Models.Request.BREB;
using BusinessLogicLayer.Models.Response.BREB;

namespace BusinessLogicLayer.ServiceInvoker
{
    public interface IBrebService
    {

        Task<BrebPaymentStatusResponse?> GetPaymentStatusAsync(BrebPaymentStatusRequest request, int clientId, string BillerId);
        Task<BrebDisputeResponse?> UpdateDisputeAsync(BrebDisputeRequest request, int clientId, string BillerId);
    }
}
