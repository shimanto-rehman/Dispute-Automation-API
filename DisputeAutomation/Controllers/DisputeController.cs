using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly ILogger<DisputeController> _logger;
        private readonly IClientManager clientManager;

        public DisputeController( ILogger<DisputeController> logger, IClientManager clientManager)
        {
            _logger = logger;
            this.clientManager = clientManager;
        }
        [HttpPost("process-collection")]
        public async Task<IActionResult> ProcessCollection([FromBody] ProcessCollectionRequest collectionRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(collectionRequest.BillerId))
                {
                    return BadRequest(new ProcessCollectionResponse
                    {
                        Success = false,
                        Message = "CollFrom is required",
                        Status = "INVALID_REQUEST"
                    });
                }

                string clientType = collectionRequest.ClientType.ToString();

                _logger.LogInformation("[Dispute Controller] Processing collection request for Client: {ClientType}, CollFrom: {CollFrom}",
                    clientType, collectionRequest.BillerId);

                var processor = "";

                var result = await clientManager.ProcessCollectionAsync(collectionRequest);

                if (result!=null)
                {
                   
                }
                else
                {
                  
                }
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "[Dispute Controller] Unsupported client type");
                return BadRequest(new ProcessCollectionResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Status = "UNSUPPORTED_CLIENT"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dispute Controller] Error in ProcessCollection endpoint");
                return StatusCode(500, new ProcessCollectionResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Status = "ERROR"
                });
            }
            return Ok();
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.Now });
        }
    }
}