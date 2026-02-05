using BusinessLogicLayer.Manager;
using Microsoft.AspNetCore.Mvc;

namespace DisputeAutomation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionManager _collectionManager;

    public CollectionsController(ICollectionManager collectionManager)
    {
        _collectionManager = collectionManager;
    }

    /// <summary>
    /// Returns today's collections filtered by branch, client and status IDs.
    /// Mirrors the following query:
    /// SELECT * FROM [MTBBillCollection].[dbo].[Collection]
    /// WHERE DATEDIFF(day, CollDate, GETDATE()) = 0
    ///   AND CollStatusId IN (1)
    ///   AND BranchCode = '0502'
    ///   AND ClientId = 31
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayCollections(
        [FromQuery] string branchCode,
        [FromQuery] int clientId,
        [FromQuery] List<int> collStatusIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return BadRequest("BranchCode is required.");
        }

        if (collStatusIds is null || collStatusIds.Count == 0)
        {
            return BadRequest("At least one CollStatusId must be provided.");
        }

        var result = await _collectionManager
            .GetTodayCollectionsAsync(branchCode, clientId, collStatusIds, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}

