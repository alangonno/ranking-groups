using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Models;
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
    public string? Cursor { get; set; }
}

public class ListGroupEventsResponse
{
    public List<EventSummaryDto> Events { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
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
    public int CommentCount { get; set; }
    public string? ImageUrl { get; set; }
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
    private readonly ICommentRepository _commentRepository;
    private readonly AppDbContext _context;

    public ListGroupEventsHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _context = context;
    }

    public async Task<ListGroupEventsResponse> HandleAsync(ListGroupEventsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var membersResult = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        var members = membersResult.Items;
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var totalMembers = members.Count;
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);

        var pagedEvents = await _eventRepository.GetByGroupAsync(request.GroupId, request.Cursor);
        var events = pagedEvents.Items;

        // Fallback para eventos antigos criados antes da migração de deadline
        foreach (var ev in events.Where(e => e.IsPendingRemoval && !e.RemovalVoteDeadline.HasValue))
        {
            ev.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
        }
        await _context.SaveChangesAsync(ct);

        var dtos = new List<EventSummaryDto>();
        foreach (var e in events)
        {
            var commentCount = await _commentRepository.GetCommentCountByEventAsync(e.Id);
            dtos.Add(new EventSummaryDto
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
                CommentCount = commentCount,
                ImageUrl = e.ImageUrl,
                Approvals = e.Approvals.Select(a => new EventApprovalSummaryDto
                {
                    UserId = a.UserId,
                    UserName = a.User?.Name ?? string.Empty,
                    VoteType = a.VoteType.ToString(),
                    CreatedAt = a.CreatedAt
                }).ToList()
            });
        }

        return new ListGroupEventsResponse
        {
            Events = dtos,
            HasMore = pagedEvents.HasMore,
            NextCursor = pagedEvents.NextCursor
        };
    }
}
