using backend.src.Common.Exceptions;
using backend.src.Common.Models;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Rankings;

public class GetGroupFeedRequest
{
    public Guid GroupId { get; set; }
    public string? Cursor { get; set; }
}

public class GetGroupFeedResponse
{
    public List<FeedItemDto> Items { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}

public class FeedItemDto
{
    public Guid Id { get; set; }
    public string FeedItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;

    public Guid? AffectedUserId { get; set; }
    public string? AffectedUserName { get; set; }
    public string? EventStatus { get; set; }
    public string? EventType { get; set; }
    public int? ScoreBalance { get; set; }

    public int? ParticipantCount { get; set; }
    public bool? IsClosed { get; set; }
    public bool? HasCurrentUserJoined { get; set; }
    public int? CommentCount { get; set; }
}

public interface IGetGroupFeedHandler
{
    Task<GetGroupFeedResponse> HandleAsync(GetGroupFeedRequest request, CancellationToken ct);
}

public class GetGroupFeedHandler : IGetGroupFeedHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;

    public GetGroupFeedHandler(
        IEventRepository eventRepository,
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository)
    {
        _eventRepository = eventRepository;
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
    }

    public async Task<GetGroupFeedResponse> HandleAsync(GetGroupFeedRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members.Items);

        // Buscar pageSize + 1 de cada tipo para garantir que temos
        // dados suficientes para determinar se há próxima página
        var fetchCount = CursorPagination.DefaultPageSize + 1;
        var pagedEvents = await _eventRepository.GetByGroupAsync(request.GroupId, request.Cursor, fetchCount);
        var pagedSharedEvents = await _sharedEventRepository.GetByGroupAsync(request.GroupId, request.Cursor, fetchCount);

        var eventItems = new List<FeedItemDto>();
        foreach (var e in pagedEvents.Items)
        {
            var commentCount = await _commentRepository.GetCommentCountByEventAsync(e.Id);
            eventItems.Add(new FeedItemDto
            {
                Id = e.Id,
                FeedItemType = "event",
                Title = e.Title,
                Description = e.Description,
                Points = e.Points,
                CreatedAt = e.CreatedAt,
                CreatedByUserId = e.CreatedByUserId,
                CreatedByUserName = e.CreatedByUser?.Name ?? string.Empty,
                AffectedUserId = e.AffectedUserId,
                AffectedUserName = e.AffectedUser?.Name ?? string.Empty,
                EventStatus = e.Status.ToString(),
                EventType = e.Type.ToString(),
                ScoreBalance = e.Status == EventStatus.Approved
                    ? (e.Type == EventType.Negative ? -e.Points : e.Points)
                    : 0,
                CommentCount = commentCount
            });
        }

        var sharedItems = new List<FeedItemDto>();
        foreach (var se in pagedSharedEvents.Items)
        {
            var commentCount = await _commentRepository.GetCommentCountBySharedEventAsync(se.Id);
            sharedItems.Add(new FeedItemDto
            {
                Id = se.Id,
                FeedItemType = "shared_event",
                Title = se.Title,
                Description = se.Description,
                Points = se.Points,
                CreatedAt = se.CreatedAt,
                CreatedByUserId = se.CreatedByUserId,
                CreatedByUserName = se.CreatedByUser?.Name ?? string.Empty,
                ParticipantCount = se.Participants.Count,
                IsClosed = se.IsClosed,
                HasCurrentUserJoined = se.Participants.Any(p => p.UserId == userId),
                CommentCount = commentCount
            });
        }

        var allItems = eventItems
            .Concat(sharedItems)
            .OrderByDescending(i => i.CreatedAt)
            .ToList();

        // Se juntos temos mais que pageSize, há próxima página
        var hasMore = allItems.Count > CursorPagination.DefaultPageSize;
        if (hasMore)
        {
            allItems.RemoveAt(allItems.Count - 1);
        }

        string? nextCursor = null;
        if (hasMore && allItems.Count > 0)
        {
            var last = allItems[^1];
            nextCursor = CursorToken.Encode(last.CreatedAt, last.Id);
        }

        return new GetGroupFeedResponse
        {
            Items = allItems,
            HasMore = hasMore,
            NextCursor = nextCursor
        };
    }
}
