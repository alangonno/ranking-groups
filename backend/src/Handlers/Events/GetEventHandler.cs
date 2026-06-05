using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class GetEventRequest
{
    public Guid EventId { get; set; }
}

public class GetEventResponse
{
    public Guid EventId { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public Guid AffectedUserId { get; set; }
    public string AffectedUserName { get; set; } = string.Empty;
    public bool IsPendingRemoval { get; set; }
    public DateTime? RemovalVoteDeadline { get; set; }
    public int QuorumRequired { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public int CommentCount { get; set; }
    public string? ImageUrl { get; set; }
    public List<EventApprovalDto> Approvals { get; set; } = new();
}

public class EventApprovalDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string VoteType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IGetEventHandler
{
    Task<GetEventResponse> HandleAsync(GetEventRequest request, CancellationToken ct);
}

public class GetEventHandler : IGetEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventApprovalRepository _eventApprovalRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly AppDbContext _context;

    public GetEventHandler(
        IEventRepository eventRepository,
        IEventApprovalRepository eventApprovalRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        ICommentRepository commentRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _eventApprovalRepository = eventApprovalRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _commentRepository = commentRepository;
        _context = context;
    }

    public async Task<GetEventResponse> HandleAsync(GetEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, @event.GroupId, members);

        // Fallback para eventos antigos criados antes da migração de deadline
        if (@event.IsPendingRemoval && !@event.RemovalVoteDeadline.HasValue)
        {
            @event.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
            _eventRepository.Update(@event);
            await _context.SaveChangesAsync(ct);
        }

        // Resolve votação expirada on-demand
        if (@event.IsPendingRemoval && @event.RemovalVoteDeadline.HasValue && DateTime.UtcNow > @event.RemovalVoteDeadline.Value)
        {
            var existingApprovals = await _eventApprovalRepository.GetByEventAsync(request.EventId);
            var resolution = EventRemovalRules.ResolveExpiredRemovalVote(@event, members, existingApprovals);
            await ResolveExpiredRemovalAsync(@event, userId, resolution, members, existingApprovals, ct);
        }

        var approvals = await _eventApprovalRepository.GetByEventAsync(request.EventId);
        var removeCount = approvals.Count(a => a.VoteType == EventVoteType.Remove);
        var keepCount = approvals.Count(a => a.VoteType == EventVoteType.Keep);
        var totalMembers = members.Count();
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);
        var commentCount = await _commentRepository.GetCommentCountByEventAsync(request.EventId);

        return new GetEventResponse
        {
            EventId = @event.Id,
            GroupId = @event.GroupId,
            Title = @event.Title,
            Description = @event.Description,
            Points = @event.Points,
            Type = @event.Type.ToString(),
            Status = @event.Status.ToString(),
            CreatedAt = @event.CreatedAt,
            CreatedByUserId = @event.CreatedByUserId,
            CreatedByUserName = @event.CreatedByUser?.Name ?? string.Empty,
            AffectedUserId = @event.AffectedUserId,
            AffectedUserName = @event.AffectedUser?.Name ?? string.Empty,
            IsPendingRemoval = @event.IsPendingRemoval,
            RemovalVoteDeadline = @event.RemovalVoteDeadline,
            QuorumRequired = quorum,
            RemoveCount = removeCount,
            KeepCount = keepCount,
            CommentCount = commentCount,
            ImageUrl = @event.ImageUrl,
            Approvals = approvals.Select(a => new EventApprovalDto
            {
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                VoteType = a.VoteType.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    private async Task ResolveExpiredRemovalAsync(
        Event @event, Guid userId, RemovalResolution resolution,
        IEnumerable<GroupMember> members, IEnumerable<EventApproval> existingApprovals, CancellationToken ct)
    {
        var votedUserIds = existingApprovals
            .Where(a => a.VoteType == EventVoteType.Remove || a.VoteType == EventVoteType.Keep)
            .Select(a => a.UserId)
            .ToHashSet();
        var nonVoters = members.Where(m => !votedUserIds.Contains(m.UserId)).ToList();

        // Registra não-votantes como Keep para auditoria
        foreach (var nonVoter in nonVoters)
        {
            _context.EventApprovals.Add(new EventApproval
            {
                EventId = @event.Id,
                UserId = nonVoter.UserId,
                VoteType = EventVoteType.Keep
            });
        }

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
            var removedLog = AuditLogBuilder.EventRemovedByVote(@event, userId, revertedPoints);
            _auditLogRepository.Add(removedLog);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            @event.IsPendingRemoval = false;
            @event.RemovalVoteDeadline = null;
            _eventRepository.Update(@event);

            await _context.SaveChangesAsync(ct);

            var cancelledLog = AuditLogBuilder.EventRemovalCancelled(@event, userId);
            _auditLogRepository.Add(cancelledLog);
            await _context.SaveChangesAsync(ct);
        }
    }
}
