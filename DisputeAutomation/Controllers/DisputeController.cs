using BusinessLogicLayer.Models.Request;
using BusinessLogicLayer.Models.Response;
using BusinessLogicLayer.Processors;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers
{
    /// <summary>
    /// Common Dispute Controller for all clients (BREB, WASA, DESCO, DPDC)
    /// Routes requests to appropriate client-specific processor
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly ProcessorFactory _processorFactory;
        private readonly ILogger<DisputeController> _logger;

        public DisputeController(ProcessorFactory processorFactory, ILogger<DisputeController> logger)
        {
            _processorFactory = processorFactory;
            _logger = logger;
        }

        /// <summary>
        /// Process collection for any client (BREB, WASA, DESCO, DPDC)
        /// </summary>
        /// <param name="request">Collection processing request with ClientType</param>
        /// <returns>Processing result</returns>
        [HttpPost("process-collection")]
        public async Task<IActionResult> ProcessCollection([FromBody] ProcessCollectionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CollFrom))
                {
                    return BadRequest(new ProcessCollectionResponse
                    {
                        Success = false,
                        Message = "CollFrom is required",
                        Status = "INVALID_REQUEST"
                    });
                }

                string clientType = request.ClientType.ToString();

                _logger.LogInformation("[Dispute Controller] Processing collection request for Client: {ClientType}, CollFrom: {CollFrom}",
                    clientType, request.CollFrom);

                // Get the appropriate processor for this client type
                var processor = _processorFactory.GetProcessor(clientType);

                // Process the collection
                var result = await processor.ProcessCollectionAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("[Dispute Controller] Collection processed successfully for Client: {ClientType}, CollectionId: {CollectionId}",
                        clientType, result.CollectionId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("[Dispute Controller] Collection processing failed for Client: {ClientType}, Status: {Status}",
                        clientType, result.Status);

                    // Return appropriate status code based on the error
                    return result.Status switch
                    {
                        "NOT_FOUND" => NotFound(result),
                        "INVALID_STATUS" => BadRequest(result),
                        _ => StatusCode(500, result)
                    };
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
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.Now });
        }
    }
}