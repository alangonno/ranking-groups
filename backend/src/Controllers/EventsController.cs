using backend.src.Common.Exceptions;
using backend.src.Handlers.Comments;
using backend.src.Handlers.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ICreateEventHandler _createEventHandler;
    private readonly IGetEventHandler _getEventHandler;
    private readonly IListGroupEventsHandler _listGroupEventsHandler;
    private readonly IListUserGroupEventsHandler _listUserGroupEventsHandler;
    private readonly IUpdateEventHandler _updateEventHandler;
    private readonly IDeleteEventHandler _deleteEventHandler;
    private readonly IVoteEventHandler _voteEventHandler;
    private readonly IRequestEventRemovalHandler _requestEventRemovalHandler;
    private readonly ICreateCommentHandler _createCommentHandler;
    private readonly IGetEventCommentsHandler _getEventCommentsHandler;

    public EventsController(
        ICreateEventHandler createEventHandler,
        IGetEventHandler getEventHandler,
        IListGroupEventsHandler listGroupEventsHandler,
        IListUserGroupEventsHandler listUserGroupEventsHandler,
        IUpdateEventHandler updateEventHandler,
        IDeleteEventHandler deleteEventHandler,
        IVoteEventHandler voteEventHandler,
        IRequestEventRemovalHandler requestEventRemovalHandler,
        ICreateCommentHandler createCommentHandler,
        IGetEventCommentsHandler getEventCommentsHandler)
    {
        _createEventHandler = createEventHandler;
        _getEventHandler = getEventHandler;
        _listGroupEventsHandler = listGroupEventsHandler;
        _listUserGroupEventsHandler = listUserGroupEventsHandler;
        _updateEventHandler = updateEventHandler;
        _deleteEventHandler = deleteEventHandler;
        _voteEventHandler = voteEventHandler;
        _requestEventRemovalHandler = requestEventRemovalHandler;
        _createCommentHandler = createCommentHandler;
        _getEventCommentsHandler = getEventCommentsHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var response = await _createEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken ct)
    {
        var request = new GetEventRequest { EventId = eventId };
        var response = await _getEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("group/{groupId:guid}")]
    public async Task<IActionResult> ListByGroup(Guid groupId, CancellationToken ct)
    {
        var request = new ListGroupEventsRequest { GroupId = groupId };
        var response = await _listGroupEventsHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("group/{groupId:guid}/user/{userId:guid}")]
    public async Task<IActionResult> ListByUser(Guid groupId, Guid userId, CancellationToken ct)
    {
        var request = new ListUserGroupEventsRequest { GroupId = groupId, UserId = userId };
        var response = await _listUserGroupEventsHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPut("{eventId:guid}")]
    public async Task<IActionResult> Update(Guid eventId, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        request.EventId = eventId;
        var response = await _updateEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> Delete(Guid eventId, CancellationToken ct)
    {
        var request = new DeleteEventRequest { EventId = eventId };
        var response = await _deleteEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{eventId:guid}/vote")]
    public async Task<IActionResult> Vote(Guid eventId, [FromBody] VoteEventRequest request, CancellationToken ct)
    {
        request.EventId = eventId;
        var response = await _voteEventHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{eventId:guid}/request-removal")]
    public async Task<IActionResult> RequestRemoval(Guid eventId, CancellationToken ct)
    {
        var request = new RequestEventRemovalRequest { EventId = eventId };
        var response = await _requestEventRemovalHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{eventId:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid eventId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        request.EventId = eventId;
        var response = await _createCommentHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{eventId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid eventId, CancellationToken ct)
    {
        var request = new GetEventCommentsRequest { EventId = eventId };
        var response = await _getEventCommentsHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
