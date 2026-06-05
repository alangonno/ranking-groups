using backend.src.Common.Exceptions;
using backend.src.Handlers.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IGetNotificationsHandler _getNotificationsHandler;
    private readonly IMarkNotificationAsReadHandler _markNotificationAsReadHandler;
    private readonly IMarkAllNotificationsAsReadHandler _markAllNotificationsAsReadHandler;

    public NotificationsController(
        IGetNotificationsHandler getNotificationsHandler,
        IMarkNotificationAsReadHandler markNotificationAsReadHandler,
        IMarkAllNotificationsAsReadHandler markAllNotificationsAsReadHandler)
    {
        _getNotificationsHandler = getNotificationsHandler;
        _markNotificationAsReadHandler = markNotificationAsReadHandler;
        _markAllNotificationsAsReadHandler = markAllNotificationsAsReadHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var request = new GetNotificationsRequest { GroupId = groupId };
        var response = await _getNotificationsHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct)
    {
        var request = new MarkNotificationAsReadRequest { NotificationId = notificationId };
        var response = await _markNotificationAsReadHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var request = new MarkAllNotificationsAsReadRequest { GroupId = groupId };
        var response = await _markAllNotificationsAsReadHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
