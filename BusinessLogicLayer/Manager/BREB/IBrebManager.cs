using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Models.DB;

namespace BusinessLogicLayer.Manager.BREB
{
    public interface  IBrebManager
    {
        Task <BrebBillCollectionModel> getBrebBillCollectionByCollectionId (long collectionId, int clientId);
        Task<BrebPaymentStatusLogModel?> getPaymentStatusRefNoAsync(string transactionId, int clientId);
        Task<bool> updateCollectionTable(CollectionModel up_collection);
    }
}
