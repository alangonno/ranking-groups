using backend.src.Common;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Handlers.Jobs;

public class CloseExpiredSharedEventsResult
{
    public int ClosedCount { get; set; }
}

public interface ICloseExpiredSharedEventsJobHandler
{
    Task<CloseExpiredSharedEventsResult> ProcessAsync(DateTime cutoff, CancellationToken ct);
}

public class CloseExpiredSharedEventsJobHandler : ICloseExpiredSharedEventsJobHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;

    public CloseExpiredSharedEventsJobHandler(
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }

    public async Task<CloseExpiredSharedEventsResult> ProcessAsync(DateTime cutoff, CancellationToken ct)
    {
        var sharedEvents = await _sharedEventRepository.GetOpenSharedEventsWithExpiredClosesAtAsync(cutoff);
        var closedCount = 0;

        foreach (var sharedEvent in sharedEvents)
        {
            sharedEvent.IsClosed = true;
            _sharedEventRepository.Update(sharedEvent);
            await _context.SaveChangesAsync(ct);

            var participants = await _context.SharedEventParticipants
                .Where(p => p.SharedEventId == sharedEvent.Id)
                .ToListAsync<SharedEventParticipant>(ct);

            var auditLog = AuditLogBuilder.SharedEventClosed(sharedEvent, participants.Count, Guid.Empty);
            _auditLogRepository.Add(auditLog);
            await _context.SaveChangesAsync(ct);

            var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
            var notifications = NotificationBuilder.BuildNotifications(auditLog, members, null, sharedEvent);
            _notificationRepository.AddRange(notifications);
            await _context.SaveChangesAsync(ct);

            closedCount++;
        }

        return new CloseExpiredSharedEventsResult { ClosedCount = closedCount };
    }
}
