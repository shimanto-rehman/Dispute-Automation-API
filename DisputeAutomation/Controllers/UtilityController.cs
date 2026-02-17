using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Manager.BREB;
using BusinessLogicLayer.Models.DB;
using BusinessLogicLayer.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IClientManager clientManager;
        private readonly IBrebManager brebManager;
        public UtilityController(IClientManager clientManager, IBrebManager brebManager)
        {
            this.clientManager = clientManager;
            this.brebManager = brebManager;
        }
        [HttpPost("process-collection")]
        public async Task<IActionResult> ProcessCollection([FromBody] ProcessCollectionRequest collectionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CollectionModel? collectionModel = await clientManager.ProcessCollectionAsync(collectionRequest);
            if (collectionModel == null) {
                return Ok();
            }
            BrebBillCollectionModel brebBillCollectionModel= await brebManager.getBrebBillCollectionByCollectionId(collectionModel.CollectionId);
            return Ok(brebBillCollectionModel);

        }

    }
}
