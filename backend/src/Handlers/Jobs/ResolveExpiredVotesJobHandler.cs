using backend.src.Common;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Handlers.Jobs;

public class ResolveExpiredVotesResult
{
    public int ApprovedEventsCount { get; set; }
    public int RejectedEventsCount { get; set; }
    public int RemovedEventsCount { get; set; }
    public int KeptEventsCount { get; set; }
    public int RemovedParticipantsCount { get; set; }
    public int KeptParticipantsCount { get; set; }
}

public interface IResolveExpiredVotesJobHandler
{
    Task<ResolveExpiredVotesResult> ProcessAsync(DateTime cutoff, CancellationToken ct);
}

public class ResolveExpiredVotesJobHandler : IResolveExpiredVotesJobHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventApprovalRepository _eventApprovalRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly ISharedEventParticipantRemovalVoteRepository _voteRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;

    public ResolveExpiredVotesJobHandler(
        IEventRepository eventRepository,
        IEventApprovalRepository eventApprovalRepository,
        IGroupMemberRepository groupMemberRepository,
        ISharedEventParticipantRepository participantRepository,
        ISharedEventParticipantRemovalVoteRepository voteRepository,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _eventApprovalRepository = eventApprovalRepository;
        _groupMemberRepository = groupMemberRepository;
        _participantRepository = participantRepository;
        _voteRepository = voteRepository;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }

    public async Task<ResolveExpiredVotesResult> ProcessAsync(DateTime cutoff, CancellationToken ct)
    {
        var result = new ResolveExpiredVotesResult();

        result.ApprovedEventsCount += await ResolveExpiredApprovalVotesAsync(cutoff, ct);
        result.RejectedEventsCount += await ResolveExpiredApprovalRejectsAsync(cutoff, ct);

        var (removedEvents, keptEvents) = await ResolveExpiredEventRemovalsAsync(cutoff, ct);
        result.RemovedEventsCount += removedEvents;
        result.KeptEventsCount += keptEvents;

        var (removedParticipants, keptParticipants) = await ResolveExpiredParticipantRemovalsAsync(cutoff, ct);
        result.RemovedParticipantsCount += removedParticipants;
        result.KeptParticipantsCount += keptParticipants;

        return result;
    }

    private async Task<int> ResolveExpiredApprovalVotesAsync(DateTime cutoff, CancellationToken ct)
    {
        var events = await _eventRepository.GetPendingEventsWithExpiredApprovalDeadlineAsync(cutoff);
        var approvedCount = 0;

        foreach (var @event in events)
        {
            var approvals = await _eventApprovalRepository.GetByEventAsync(@event.Id);
            var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
            var totalMembers = members.Count();
            var approveCount = approvals.Count(a => a.VoteType == EventVoteType.Approve);
            var rejectCount = approvals.Count(a => a.VoteType == EventVoteType.Reject);

            var resolution = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers, approveCount, rejectCount);

            if (resolution == ApprovalResolution.Approve)
            {
                @event.Status = EventStatus.Approved;
                @event.ApprovedAt = DateTime.UtcNow;
                @event.ApprovalDeadline = null;
                _eventRepository.Update(@event);

                var member = await _groupMemberRepository.GetByGroupAndUserAsync(@event.GroupId, @event.AffectedUserId);
                if (member != null)
                {
                    member.CurrentScore += @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                    _groupMemberRepository.Update(member);
                }

                await _context.SaveChangesAsync(ct);

                var appliedPoints = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                var approvedLog = AuditLogBuilder.EventApproved(@event, Guid.Empty, appliedPoints);
                _auditLogRepository.Add(approvedLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(approvedLog, members, @event, null);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                approvedCount++;
            }
        }

        return approvedCount;
    }

    private async Task<int> ResolveExpiredApprovalRejectsAsync(DateTime cutoff, CancellationToken ct)
    {
        var events = await _eventRepository.GetPendingEventsWithExpiredApprovalDeadlineAsync(cutoff);
        var rejectedCount = 0;

        foreach (var @event in events)
        {
            var approvals = await _eventApprovalRepository.GetByEventAsync(@event.Id);
            var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
            var totalMembers = members.Count();
            var approveCount = approvals.Count(a => a.VoteType == EventVoteType.Approve);
            var rejectCount = approvals.Count(a => a.VoteType == EventVoteType.Reject);

            var resolution = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers, approveCount, rejectCount);

            if (resolution == ApprovalResolution.Reject)
            {
                _eventRepository.Remove(@event);
                await _context.SaveChangesAsync(ct);

                var rejectedLog = AuditLogBuilder.EventRejectedDeleted(@event, Guid.Empty);
                _auditLogRepository.Add(rejectedLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(rejectedLog, members, @event, null);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                rejectedCount++;
            }
        }

        return rejectedCount;
    }

    private async Task<(int removed, int kept)> ResolveExpiredEventRemovalsAsync(DateTime cutoff, CancellationToken ct)
    {
        var events = await _context.Events
            .Include(e => e.Group)
            .Include(e => e.CreatedByUser)
            .Include(e => e.AffectedUser)
            .Include(e => e.Approvals)
            .Where(e => e.IsPendingRemoval && e.RemovalVoteDeadline < cutoff)
            .ToListAsync(ct);

        var removedCount = 0;
        var keptCount = 0;

        foreach (var @event in events)
        {
            var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
            var approvals = await _eventApprovalRepository.GetByEventAsync(@event.Id);
            var resolution = EventRemovalRules.ResolveExpiredRemovalVote(@event, members, approvals);

            if (resolution == RemovalResolution.Remove)
            {
                if (@event.Status == EventStatus.Approved)
                {
                    var affectedMember = await _groupMemberRepository.GetByGroupAndUserAsync(@event.GroupId, @event.AffectedUserId);
                    if (affectedMember != null)
                    {
                        var revertPoints = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                        affectedMember.CurrentScore -= revertPoints;
                        _groupMemberRepository.Update(affectedMember);
                    }
                }

                _eventRepository.Remove(@event);
                await _context.SaveChangesAsync(ct);

                var revertedPoints = @event.Status == EventStatus.Approved
                    ? (@event.Type == EventType.Negative ? -@event.Points : @event.Points)
                    : 0;
                var removedLog = AuditLogBuilder.EventRemovedByVote(@event, Guid.Empty, revertedPoints);
                _auditLogRepository.Add(removedLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(removedLog, members, @event, null);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                removedCount++;
            }
            else
            {
                @event.IsPendingRemoval = false;
                @event.RemovalVoteDeadline = null;
                _eventRepository.Update(@event);

                await _context.SaveChangesAsync(ct);

                var cancelledLog = AuditLogBuilder.EventRemovalCancelled(@event, Guid.Empty);
                _auditLogRepository.Add(cancelledLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(cancelledLog, members, @event, null);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                keptCount++;
            }
        }

        return (removedCount, keptCount);
    }

    private async Task<(int removed, int kept)> ResolveExpiredParticipantRemovalsAsync(DateTime cutoff, CancellationToken ct)
    {
        var participants = await _context.SharedEventParticipants
            .Include(p => p.SharedEvent)
            .ThenInclude(se => se.Group)
            .Include(p => p.SharedEvent)
            .ThenInclude(se => se.CreatedByUser)
            .Include(p => p.User)
            .Where(p => p.IsPendingRemoval && p.RemovalVoteDeadline < cutoff)
            .ToListAsync(ct);

        var removedCount = 0;
        var keptCount = 0;

        foreach (var participant in participants)
        {
            var sharedEvent = participant.SharedEvent;
            var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
            var votes = await _voteRepository.GetByParticipantAsync(participant.Id);
            var resolution = SharedEventParticipantRemovalRules.ResolveExpiredRemovalVote(members, votes);

            if (resolution == RemovalResolution.Remove)
            {
                var member = await _groupMemberRepository.GetByGroupAndUserAsync(sharedEvent.GroupId, participant.UserId);
                if (member != null)
                {
                    member.CurrentScore -= sharedEvent.Points;
                    _groupMemberRepository.Update(member);
                }

                _participantRepository.Remove(participant);

                var votesToRemove = await _voteRepository.GetByParticipantAsync(participant.Id);
                _voteRepository.RemoveRange(votesToRemove);

                await _context.SaveChangesAsync(ct);

                var removedLog = AuditLogBuilder.SharedEventParticipantRemovedByVote(sharedEvent, participant.UserId, participant.User?.Name ?? string.Empty);
                _auditLogRepository.Add(removedLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(removedLog, members, null, sharedEvent);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                removedCount++;
            }
            else
            {
                participant.IsPendingRemoval = false;
                participant.RemovalVoteDeadline = null;
                _participantRepository.Update(participant);

                var votesToClear = await _voteRepository.GetByParticipantAsync(participant.Id);
                _voteRepository.RemoveRange(votesToClear);

                await _context.SaveChangesAsync(ct);

                var cancelledLog = AuditLogBuilder.SharedEventParticipantRemovalCancelled(sharedEvent, participant.UserId, participant.User?.Name ?? string.Empty);
                _auditLogRepository.Add(cancelledLog);
                await _context.SaveChangesAsync(ct);

                var notifications = NotificationBuilder.BuildNotifications(cancelledLog, members, null, sharedEvent);
                _notificationRepository.AddRange(notifications);
                await _context.SaveChangesAsync(ct);

                keptCount++;
            }
        }

        return (removedCount, keptCount);
    }
}
