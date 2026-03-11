using System.Threading.Tasks;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Response;

namespace BusinessLogicLayer.Manager
{

    public interface IClientManager
    {

        Task<CollectionModel?> ProcessCollectionAsync(ProcessCollectionRequest collectionRequest);
        bool CanProcess(string clientType);
    }
}