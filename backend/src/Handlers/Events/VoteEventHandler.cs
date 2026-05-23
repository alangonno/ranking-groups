using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class VoteEventRequest
{
    public Guid EventId { get; set; }
    public EventVoteType VoteType { get; set; }
}

public class VoteEventResponse
{
    public Guid EventId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ApprovalCount { get; set; }
    public bool EventApproved { get; set; }
}

public interface IVoteEventHandler
{
    Task<VoteEventResponse> HandleAsync(VoteEventRequest request, CancellationToken ct);
}

public class VoteEventHandler : IVoteEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventApprovalRepository _eventApprovalRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public VoteEventHandler(
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

    public async Task<VoteEventResponse> HandleAsync(VoteEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        var existingApprovals = await _eventApprovalRepository.GetByEventAsync(request.EventId);

        var voter = members.FirstOrDefault(m => m.UserId == userId)?.User;
        if (voter == null)
        {
            throw new BusinessRuleException("user_not_found", "Usuário não encontrado.");
        }

        var approval = new EventApproval
        {
            EventId = request.EventId,
            UserId = userId,
            VoteType = request.VoteType
        };

        EventApprovalRules.ValidateEventIsPending(@event.Status);
        EventApprovalRules.ValidateCanVote(approval, @event, voter, members);
        EventApprovalRules.ValidateNoDuplicateVote(userId, request.EventId, existingApprovals);

        _eventApprovalRepository.Add(approval);
        await _context.SaveChangesAsync(ct);

        var updatedApprovals = await _eventApprovalRepository.GetByEventAsync(request.EventId);
        var approvalCount = updatedApprovals.Count(a => a.VoteType == EventVoteType.Approve);
        var totalMembers = members.Count();
        var eventApproved = false;

        if (request.VoteType == EventVoteType.Approve)
        {
            try
            {
                EventApprovalRules.ValidateApprovalQuorum(approvalCount, totalMembers);
                @event.Status = EventStatus.Approved;
                @event.ApprovedAt = DateTime.UtcNow;
                _eventRepository.Update(@event);

                await UpdateAffectedUserScoreAsync(@event.GroupId, @event.AffectedUserId, @event.Type, @event.Points);
                eventApproved = true;

                await _context.SaveChangesAsync(ct);

                var appliedPoints = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
                var approvedLog = AuditLogBuilder.EventApproved(@event, userId, appliedPoints);
                _auditLogRepository.Add(approvedLog);
                await _context.SaveChangesAsync(ct);
            }
            catch (BusinessRuleException)
            {
                // Quorum de aprovação ainda não atingido, evento permanece pendente
            }
        }

        if (request.VoteType == EventVoteType.Reject)
        {
            var rejectionCount = updatedApprovals.Count(a => a.VoteType == EventVoteType.Reject);
            try
            {
                EventApprovalRules.ValidateApprovalQuorum(rejectionCount, totalMembers);

                // Quorum de rejeição atingido — deletar o evento
                _eventRepository.Remove(@event);
                await _context.SaveChangesAsync(ct);

                var rejectedLog = AuditLogBuilder.EventRejectedDeleted(@event, userId);
                _auditLogRepository.Add(rejectedLog);
                await _context.SaveChangesAsync(ct);

                return new VoteEventResponse
                {
                    EventId = @event.Id,
                    Status = "Deleted",
                    ApprovalCount = approvalCount,
                    EventApproved = false
                };
            }
            catch (BusinessRuleException)
            {
                // Quorum de rejeição ainda não atingido, evento permanece pendente
            }
        }

        return new VoteEventResponse
        {
            EventId = @event.Id,
            Status = @event.Status.ToString(),
            ApprovalCount = approvalCount,
            EventApproved = eventApproved
        };
    }

    private async Task UpdateAffectedUserScoreAsync(Guid groupId, Guid affectedUserId, EventType type, int points)
    {
        var member = await _groupMemberRepository.GetByGroupAndUserAsync(groupId, affectedUserId);
        if (member != null)
        {
            member.CurrentScore += type == EventType.Negative ? -points : points;
            _groupMemberRepository.Update(member);
        }
    }
}
