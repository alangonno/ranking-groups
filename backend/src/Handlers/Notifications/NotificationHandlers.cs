using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Notifications;

public class GetNotificationsRequest
{
    public Guid? GroupId { get; set; }
    public string? Cursor { get; set; }
}

public class GetNotificationsResponse
{
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? EventId { get; set; }
    public Guid? SharedEventId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetNotificationsPagedResponse
{
    public List<GetNotificationsResponse> Items { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}

public interface IGetNotificationsHandler
{
    Task<GetNotificationsPagedResponse> HandleAsync(GetNotificationsRequest request, CancellationToken ct);
}

public class GetNotificationsHandler : IGetNotificationsHandler
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetNotificationsPagedResponse> HandleAsync(GetNotificationsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var pagedNotifications = await _notificationRepository.GetByUserIdAsync(userId, request.Cursor);
        var notifications = pagedNotifications.Items;

        if (request.GroupId.HasValue)
        {
            notifications = notifications.Where(n => n.GroupId == request.GroupId.Value).ToList();
        }

        var items = notifications.Select(n => new GetNotificationsResponse
        {
            NotificationId = n.Id,
            Title = n.Title,
            Description = n.Description,
            Action = n.Action,
            EventId = n.EventId,
            SharedEventId = n.SharedEventId,
            CreatedAt = n.CreatedAt
        }).ToList();

        return new GetNotificationsPagedResponse
        {
            Items = items,
            HasMore = pagedNotifications.HasMore,
            NextCursor = pagedNotifications.NextCursor
        };
    }
}

public class MarkNotificationAsReadRequest
{
    public Guid NotificationId { get; set; }
}

public class MarkNotificationAsReadResponse
{
    public bool Success { get; set; }
}

public interface IMarkNotificationAsReadHandler
{
    Task<MarkNotificationAsReadResponse> HandleAsync(MarkNotificationAsReadRequest request, CancellationToken ct);
}

public class MarkNotificationAsReadHandler : IMarkNotificationAsReadHandler
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public MarkNotificationAsReadHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<MarkNotificationAsReadResponse> HandleAsync(MarkNotificationAsReadRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var notification = notifications.FirstOrDefault(n => n.Id == request.NotificationId);

        if (notification == null)
        {
            throw new BusinessRuleException("notification_not_found", "Notificação não encontrada.");
        }

        _notificationRepository.Remove(notification);
        await _context.SaveChangesAsync(ct);

        return new MarkNotificationAsReadResponse { Success = true };
    }
}

public class MarkAllNotificationsAsReadRequest
{
    public Guid? GroupId { get; set; }
}

public class MarkAllNotificationsAsReadResponse
{
    public int DeletedCount { get; set; }
}

public interface IMarkAllNotificationsAsReadHandler
{
    Task<MarkAllNotificationsAsReadResponse> HandleAsync(MarkAllNotificationsAsReadRequest request, CancellationToken ct);
}

public class MarkAllNotificationsAsReadHandler : IMarkAllNotificationsAsReadHandler
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public MarkAllNotificationsAsReadHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<MarkAllNotificationsAsReadResponse> HandleAsync(MarkAllNotificationsAsReadRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var count = notifications.Count();

        if (request.GroupId.HasValue)
        {
            count = notifications.Count(n => n.GroupId == request.GroupId.Value);
            if (count > 0)
            {
                var notificationsToDelete = notifications.Where(n => n.GroupId == request.GroupId.Value);
                foreach (var notification in notificationsToDelete)
                {
                    _notificationRepository.Remove(notification);
                }
            }
        }
        else
        {
            _notificationRepository.RemoveByUserId(userId);
        }

        await _context.SaveChangesAsync(ct);

        return new MarkAllNotificationsAsReadResponse { DeletedCount = count };
    }
}
