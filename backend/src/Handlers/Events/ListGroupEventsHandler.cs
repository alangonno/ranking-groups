using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class ListGroupEventsRequest
{
    public Guid GroupId { get; set; }
}

public class ListGroupEventsResponse
{
    public List<EventSummaryDto> Events { get; set; } = new();
}

public class EventSummaryDto
{
    public Guid EventId { get; set; }
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
    public int ApprovalCount { get; set; }
    public bool IsPendingRemoval { get; set; }
    public DateTime? RemovalVoteDeadline { get; set; }
    public int QuorumRequired { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public List<EventApprovalSummaryDto> Approvals { get; set; } = new();
}

public class EventApprovalSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string VoteType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IListGroupEventsHandler
{
    Task<ListGroupEventsResponse> HandleAsync(ListGroupEventsRequest request, CancellationToken ct);
}

public class ListGroupEventsHandler : IListGroupEventsHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public ListGroupEventsHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<ListGroupEventsResponse> HandleAsync(ListGroupEventsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var totalMembers = members.Count();
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);

        // Fallback para eventos antigos criados antes da migração de deadline
        foreach (var ev in events.Where(e => e.IsPendingRemoval && !e.RemovalVoteDeadline.HasValue))
        {
            ev.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
        }
        await _context.SaveChangesAsync(ct);

        var dtos = events.Select(e => new EventSummaryDto
        {
            EventId = e.Id,
            Title = e.Title,
            Description = e.Description,
            Points = e.Points,
            Type = e.Type.ToString(),
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            CreatedByUserId = e.CreatedByUserId,
            CreatedByUserName = e.CreatedByUser?.Name ?? string.Empty,
            AffectedUserId = e.AffectedUserId,
            AffectedUserName = e.AffectedUser?.Name ?? string.Empty,
            ApprovalCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Approve),
            IsPendingRemoval = e.IsPendingRemoval,
            RemovalVoteDeadline = e.RemovalVoteDeadline,
            QuorumRequired = quorum,
            RemoveCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Remove),
            KeepCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Keep),
            Approvals = e.Approvals.Select(a => new EventApprovalSummaryDto
            {
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                VoteType = a.VoteType.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList()
        }).ToList();

        return new ListGroupEventsResponse { Events = dtos };
    }
}
