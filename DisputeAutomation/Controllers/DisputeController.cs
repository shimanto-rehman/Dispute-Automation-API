using System.Data;
using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly DisputeManager _disputeManager;

        public DisputeController(DisputeManager disputeManager)
        {
            _disputeManager = disputeManager;
        }

        [HttpPost("today-collections")]
        public async Task<IActionResult> GetTodayCollections(
            [FromBody] TodayCollectionRequest request)
        {
            List<CollectionModel>? result = await _disputeManager.GetTodayCollectionsAsync(
                request
            );
            List<BrebBillCollectionModel> result2 = new List<BrebBillCollectionModel>();
            if(result != null)
            {
                foreach (var item in result)
                {
                    BrebBillCollectionModel dt = await _disputeManager.GetBrebBillCollectionAsync(
                item.CollectionId.ToString());
                    if(dt != null)
                    {
                        result2.Add(dt);
                    }

                }
                
            }
            return Ok(result2);
        }

        [HttpPost("breb")]
        public async Task<IActionResult> GetBrebBill(
            [FromBody] BrebBillRequest request)
        {
            BrebBillCollectionModel dt = await _disputeManager.GetBrebBillCollectionAsync(
                request.CollectionId.ToString());
            return Ok(dt);
        }
    }
}
