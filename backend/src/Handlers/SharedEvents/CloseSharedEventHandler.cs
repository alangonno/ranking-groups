using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class CloseSharedEventRequest
{
    public Guid SharedEventId { get; set; }
}

public class CloseSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public bool IsClosed { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime ClosedAt { get; set; }
}

public interface ICloseSharedEventHandler
{
    Task<CloseSharedEventResponse> HandleAsync(CloseSharedEventRequest request, CancellationToken ct);
}

public class CloseSharedEventHandler : ICloseSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;

    public CloseSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }

    public async Task<CloseSharedEventResponse> HandleAsync(CloseSharedEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId);
        if (sharedEvent == null)
        {
            throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members);

        SharedEventRules.ValidateCanClose(sharedEvent.IsClosed);
        SharedEventRules.ValidateUserCanCloseSharedEvent(userId, sharedEvent.CreatedByUserId, members);

        sharedEvent.IsClosed = true;
        _sharedEventRepository.Update(sharedEvent);
        await _context.SaveChangesAsync(ct);

        var participants = await _participantRepository.GetBySharedEventAsync(request.SharedEventId);
        var auditLog = AuditLogBuilder.SharedEventClosed(sharedEvent, participants.Count(), userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        var notifications = NotificationBuilder.BuildNotifications(auditLog, members, null, sharedEvent);
        _notificationRepository.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        return new CloseSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            IsClosed = sharedEvent.IsClosed,
            ParticipantCount = participants.Count(),
            ClosedAt = DateTime.UtcNow
        };
    }
}
