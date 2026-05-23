using backend.src.Common.Exceptions;
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

    public SharedEventsController(
        ICreateSharedEventHandler createSharedEventHandler,
        IGetSharedEventHandler getSharedEventHandler,
        IListGroupSharedEventsHandler listGroupSharedEventsHandler,
        IUpdateSharedEventHandler updateSharedEventHandler,
        IDeleteSharedEventHandler deleteSharedEventHandler,
        IJoinSharedEventHandler joinSharedEventHandler,
        ILeaveSharedEventHandler leaveSharedEventHandler,
        ICloseSharedEventHandler closeSharedEventHandler)
    {
        _createSharedEventHandler = createSharedEventHandler;
        _getSharedEventHandler = getSharedEventHandler;
        _listGroupSharedEventsHandler = listGroupSharedEventsHandler;
        _updateSharedEventHandler = updateSharedEventHandler;
        _deleteSharedEventHandler = deleteSharedEventHandler;
        _joinSharedEventHandler = joinSharedEventHandler;
        _leaveSharedEventHandler = leaveSharedEventHandler;
        _closeSharedEventHandler = closeSharedEventHandler;
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
    public async Task<IActionResult> ListByGroup(Guid groupId, CancellationToken ct)
    {
        var request = new ListGroupSharedEventsRequest { GroupId = groupId };
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
}
