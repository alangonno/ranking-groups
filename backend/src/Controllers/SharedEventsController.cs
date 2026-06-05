using backend.src.Common.Exceptions;
using backend.src.Handlers.Comments;
using backend.src.Handlers.SharedEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/shared-events")]
[Authorize]
public class SharedEventsController : ControllerBase
{
    private readonly ICreateSharedEventHandler _createSharedEventHandler;
    private readonly IGetSharedEventHandler _getSharedEventHandler;
    private readonly IListGroupSharedEventsHandler _listGroupSharedEventsHandler;
    private readonly IUpdateSharedEventHandler _updateSharedEventHandler;
    private readonly IDeleteSharedEventHandler _deleteSharedEventHandler;
    private readonly IJoinSharedEventHandler _joinSharedEventHandler;
    private readonly ILeaveSharedEventHandler _leaveSharedEventHandler;
    private readonly ICloseSharedEventHandler _closeSharedEventHandler;
    private readonly IRequestSharedEventParticipantRemovalHandler _requestRemovalHandler;
    private readonly IVoteSharedEventParticipantRemovalHandler _voteRemovalHandler;
    private readonly ICreateCommentHandler _createCommentHandler;
    private readonly IGetSharedEventCommentsHandler _getSharedEventCommentsHandler;

    public SharedEventsController(
        ICreateSharedEventHandler createSharedEventHandler,
        IGetSharedEventHandler getSharedEventHandler,
        IListGroupSharedEventsHandler listGroupSharedEventsHandler,
        IUpdateSharedEventHandler updateSharedEventHandler,
        IDeleteSharedEventHandler deleteSharedEventHandler,
        IJoinSharedEventHandler joinSharedEventHandler,
        ILeaveSharedEventHandler leaveSharedEventHandler,
        ICloseSharedEventHandler closeSharedEventHandler,
        IRequestSharedEventParticipantRemovalHandler requestRemovalHandler,
        IVoteSharedEventParticipantRemovalHandler voteRemovalHandler,
        ICreateCommentHandler createCommentHandler,
        IGetSharedEventCommentsHandler getSharedEventCommentsHandler)
    {
        _createSharedEventHandler = createSharedEventHandler;
        _getSharedEventHandler = getSharedEventHandler;
        _listGroupSharedEventsHandler = listGroupSharedEventsHandler;
        _updateSharedEventHandler = updateSharedEventHandler;
        _deleteSharedEventHandler = deleteSharedEventHandler;
        _joinSharedEventHandler = joinSharedEventHandler;
        _leaveSharedEventHandler = leaveSharedEventHandler;
        _closeSharedEventHandler = closeSharedEventHandler;
        _requestRemovalHandler = requestRemovalHandler;
        _voteRemovalHandler = voteRemovalHandler;
        _createCommentHandler = createCommentHandler;
        _getSharedEventCommentsHandler = getSharedEventCommentsHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSharedEventRequest request, CancellationToken ct)
    {
        var response = await _createSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{sharedEventId:guid}")]
    public async Task<IActionResult> Get(Guid sharedEventId, CancellationToken ct)
    {
        var request = new GetSharedEventRequest { SharedEventId = sharedEventId };
        var response = await _getSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("group/{groupId:guid}")]
    public async Task<IActionResult> ListByGroup(Guid groupId, CancellationToken ct, [FromQuery] string? cursor)
    {
        var request = new ListGroupSharedEventsRequest { GroupId = groupId, Cursor = cursor };
        var response = await _listGroupSharedEventsHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPut("{sharedEventId:guid}")]
    public async Task<IActionResult> Update(Guid sharedEventId, [FromBody] UpdateSharedEventRequest request, CancellationToken ct)
    {
        request.SharedEventId = sharedEventId;
        var response = await _updateSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpDelete("{sharedEventId:guid}")]
    public async Task<IActionResult> Delete(Guid sharedEventId, CancellationToken ct)
    {
        var request = new DeleteSharedEventRequest { SharedEventId = sharedEventId };
        var response = await _deleteSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/join")]
    public async Task<IActionResult> Join(Guid sharedEventId, CancellationToken ct)
    {
        var request = new JoinSharedEventRequest { SharedEventId = sharedEventId };
        var response = await _joinSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/leave")]
    public async Task<IActionResult> Leave(Guid sharedEventId, CancellationToken ct)
    {
        var request = new LeaveSharedEventRequest { SharedEventId = sharedEventId };
        var response = await _leaveSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/close")]
    public async Task<IActionResult> Close(Guid sharedEventId, CancellationToken ct)
    {
        var request = new CloseSharedEventRequest { SharedEventId = sharedEventId };
        var response = await _closeSharedEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/participants/{participantId:guid}/request-removal")]
    public async Task<IActionResult> RequestParticipantRemoval(Guid sharedEventId, Guid participantId, CancellationToken ct)
    {
        var request = new RequestSharedEventParticipantRemovalRequest
        {
            SharedEventId = sharedEventId,
            ParticipantId = participantId
        };
        var response = await _requestRemovalHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/participants/{participantId:guid}/vote")]
    public async Task<IActionResult> VoteParticipantRemoval(Guid sharedEventId, Guid participantId, [FromBody] VoteSharedEventParticipantRemovalRequest request, CancellationToken ct)
    {
        request.SharedEventId = sharedEventId;
        request.ParticipantId = participantId;
        var response = await _voteRemovalHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{sharedEventId:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid sharedEventId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        request.SharedEventId = sharedEventId;
        var response = await _createCommentHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{sharedEventId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid sharedEventId, CancellationToken ct, [FromQuery] string? cursor)
    {
        var request = new GetSharedEventCommentsRequest { SharedEventId = sharedEventId, Cursor = cursor };
        var response = await _getSharedEventCommentsHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
