using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class ListUserGroupEventsRequest
{
    public Guid GroupId { get; set; }
    public Guid? UserId { get; set; }
    public string? Cursor { get; set; }
}

public class ListUserGroupEventsResponse
{
    public List<UserEventSummaryDto> Events { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}

    public class UserEventSummaryDto
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
        public int ScoreBalance { get; set; }
        public int CommentCount { get; set; }
    }

public interface IListUserGroupEventsHandler
{
    Task<ListUserGroupEventsResponse> HandleAsync(ListUserGroupEventsRequest request, CancellationToken ct);
}

public class ListUserGroupEventsHandler : IListUserGroupEventsHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;

    public ListUserGroupEventsHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
    }

    public async Task<ListUserGroupEventsResponse> HandleAsync(ListUserGroupEventsRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var targetUserId = request.UserId ?? currentUserId;

        var membersResult = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        var members = membersResult.Items;
        GroupPermissionRules.ValidateUserCanInteract(currentUserId, request.GroupId, members);

        var pagedEvents = await _eventRepository.GetByGroupAsync(request.GroupId, request.Cursor);
        var events = pagedEvents.Items;
        var userEvents = events
            .Where(e => e.AffectedUserId == targetUserId)
            .OrderBy(e => e.CreatedAt)
            .ToList();

        var runningBalance = 0;
        var dtos = new List<UserEventSummaryDto>();

        foreach (var @event in userEvents)
        {
            var impact = @event.Status == EventStatus.Approved
                ? (@event.Type == EventType.Negative ? -@event.Points : @event.Points)
                : 0;

            var commentCount = await _commentRepository.GetCommentCountByEventAsync(@event.Id);

            dtos.Add(new UserEventSummaryDto
            {
                EventId = @event.Id,
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
                ScoreBalance = runningBalance,
                CommentCount = commentCount
            });

            runningBalance += impact;
        }

        return new ListUserGroupEventsResponse
        {
            Events = dtos,
            HasMore = pagedEvents.HasMore,
            NextCursor = pagedEvents.NextCursor
        };
    }
}
