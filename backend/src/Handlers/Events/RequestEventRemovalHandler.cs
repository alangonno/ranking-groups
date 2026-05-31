using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Handlers.Events;

public class RequestEventRemovalRequest
{
    public Guid EventId { get; set; }
}

public class RequestEventRemovalResponse
{
    public Guid EventId { get; set; }
    public bool IsPendingRemoval { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public int QuorumRequired { get; set; }
}

public interface IRequestEventRemovalHandler
{
    Task<RequestEventRemovalResponse> HandleAsync(RequestEventRemovalRequest request, CancellationToken ct);
}

public class RequestEventRemovalHandler : IRequestEventRemovalHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventApprovalRepository _eventApprovalRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public RequestEventRemovalHandler(
        IEventRepository eventRepository,
        IEventApprovalRepository eventApprovalRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _eventApprovalRepository = eventApprovalRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<RequestEventRemovalResponse> HandleAsync(RequestEventRemovalRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        EventRemovalRules.ValidateCanInitiateRemoval(@event, userId, members);

        @event.IsPendingRemoval = true;
        _eventRepository.Update(@event);

        var totalMembers = members.Count();
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);

        _context.EventApprovals.Add(new EventApproval
        {
            EventId = @event.Id,
            UserId = @event.CreatedByUserId,
            VoteType = EventVoteType.Keep
        });

        if (userId != @event.CreatedByUserId)
        {
            _context.EventApprovals.Add(new EventApproval
            {
                EventId = @event.Id,
                UserId = userId,
                VoteType = EventVoteType.Remove
            });
        }

        await _context.SaveChangesAsync(ct);

        var existingApprovals = await _eventApprovalRepository.GetByEventAsync(request.EventId);
        var removeCount = existingApprovals.Count(a => a.VoteType == EventVoteType.Remove);
        var keepCount = existingApprovals.Count(a => a.VoteType == EventVoteType.Keep);

        if (removeCount >= quorum && removeCount > keepCount)
        {
            if (@event.Status == EventStatus.Approved)
            {
                var member = await _groupMemberRepository.GetByGroupAndUserAsync(@event.GroupId, @event.AffectedUserId);
                if (member != null)
                {
                    var revertPoints = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                    member.CurrentScore -= revertPoints;
                    _groupMemberRepository.Update(member);
                }
            }

            _eventRepository.Remove(@event);

            await _context.SaveChangesAsync(ct);

            var revertedPoints = @event.Status == EventStatus.Approved
                ? (@event.Type == EventType.Negative ? -@event.Points : @event.Points)
                : 0;
            var removedLog = AuditLogBuilder.EventRemovedByVote(@event, userId, revertedPoints);
            _auditLogRepository.Add(removedLog);
        }
        else if (keepCount >= quorum && keepCount > removeCount)
        {
            @event.IsPendingRemoval = false;
            _eventRepository.Update(@event);

            await _context.SaveChangesAsync(ct);

            var cancelledLog = AuditLogBuilder.EventRemovalCancelled(@event, userId);
            _auditLogRepository.Add(cancelledLog);
        }
        else
        {
            var initiatedLog = AuditLogBuilder.EventRemovalInitiated(@event, userId);
            _auditLogRepository.Add(initiatedLog);
        }

        await _context.SaveChangesAsync(ct);

        return new RequestEventRemovalResponse
        {
            EventId = @event.Id,
            IsPendingRemoval = @event.IsPendingRemoval,
            RemoveCount = removeCount,
            KeepCount = keepCount,
            QuorumRequired = quorum
        };
    }
}
