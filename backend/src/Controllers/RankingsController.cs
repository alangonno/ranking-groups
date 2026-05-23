using backend.src.Common.Exceptions;
using backend.src.Handlers.Rankings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/rankings")]
[Authorize]
public class RankingsController : ControllerBase
{
    private readonly IGetGroupRankingHandler _getGroupRankingHandler;
    private readonly IGetGroupFeedHandler _getGroupFeedHandler;

    public RankingsController(
        IGetGroupRankingHandler getGroupRankingHandler,
        IGetGroupFeedHandler getGroupFeedHandler)
    {
        _getGroupRankingHandler = getGroupRankingHandler;
        _getGroupFeedHandler = getGroupFeedHandler;
    }

    [HttpGet("group/{groupId:guid}")]
    public async Task<IActionResult> GetRanking(
        Guid groupId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var request = new GetGroupRankingRequest
        {
            GroupId = groupId,
            FromDate = fromDate,
            ToDate = toDate
        };
        var response = await _getGroupRankingHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("group/{groupId:guid}/feed")]
    public async Task<IActionResult> GetFeed(Guid groupId, CancellationToken ct, [FromQuery] int limit = 20)
    {
        var request = new GetGroupFeedRequest
        {
            GroupId = groupId,
            Limit = limit
        };
        var response = await _getGroupFeedHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
