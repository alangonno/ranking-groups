using backend.src.Common.Exceptions;
using backend.src.Handlers.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly ICreateGroupHandler _createGroupHandler;
    private readonly IJoinGroupHandler _joinGroupHandler;
    private readonly IListUserGroupsHandler _listUserGroupsHandler;
    private readonly IGetGroupDetailsHandler _getGroupDetailsHandler;
    private readonly ILeaveGroupHandler _leaveGroupHandler;
    private readonly IGetUserGroupProfileHandler _getUserGroupProfileHandler;

    public GroupsController(
        ICreateGroupHandler createGroupHandler,
        IJoinGroupHandler joinGroupHandler,
        IListUserGroupsHandler listUserGroupsHandler,
        IGetGroupDetailsHandler getGroupDetailsHandler,
        ILeaveGroupHandler leaveGroupHandler,
        IGetUserGroupProfileHandler getUserGroupProfileHandler)
    {
        _createGroupHandler = createGroupHandler;
        _joinGroupHandler = joinGroupHandler;
        _listUserGroupsHandler = listUserGroupsHandler;
        _getGroupDetailsHandler = getGroupDetailsHandler;
        _leaveGroupHandler = leaveGroupHandler;
        _getUserGroupProfileHandler = getUserGroupProfileHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request, CancellationToken ct)
    {
        var response = await _createGroupHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinGroupRequest request, CancellationToken ct)
    {
        var response = await _joinGroupHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct)
    {
        var response = await _listUserGroupsHandler.HandleAsync(ct);
        return Ok(response);
    }

    [HttpGet("{groupId:guid}")]
    public async Task<IActionResult> GetDetails(Guid groupId, CancellationToken ct)
    {
        var request = new GetGroupDetailsRequest { GroupId = groupId };
        var response = await _getGroupDetailsHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("{groupId:guid}/leave")]
    public async Task<IActionResult> Leave(Guid groupId, [FromBody] LeaveGroupRequest request, CancellationToken ct)
    {
        request.GroupId = groupId;
        var response = await _leaveGroupHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{groupId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> GetUserProfile(Guid groupId, Guid userId, CancellationToken ct)
    {
        var request = new GetUserGroupProfileRequest { GroupId = groupId, UserId = userId };
        var response = await _getUserGroupProfileHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
