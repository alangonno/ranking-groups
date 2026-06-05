using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class ListGroupSharedEventsRequest
{
    public Guid GroupId { get; set; }
    public string? Cursor { get; set; }
}

public class ListGroupSharedEventsResponse
{
    public List<SharedEventSummaryDto> SharedEvents { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}

public class SharedEventSummaryDto
{
    public Guid SharedEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosesAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public bool HasCurrentUserJoined { get; set; }
    public int CommentCount { get; set; }
}

public interface IListGroupSharedEventsHandler
{
    Task<ListGroupSharedEventsResponse> HandleAsync(ListGroupSharedEventsRequest request, CancellationToken ct);
}

public class ListGroupSharedEventsHandler : IListGroupSharedEventsHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;

    public ListGroupSharedEventsHandler(
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository)
    {
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
    }

    public async Task<ListGroupSharedEventsResponse> HandleAsync(ListGroupSharedEventsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members.Items);

        var pagedSharedEvents = await _sharedEventRepository.GetByGroupAsync(request.GroupId, request.Cursor);

        var dtos = new List<SharedEventSummaryDto>();
        foreach (var se in pagedSharedEvents.Items)
        {
            var commentCount = await _commentRepository.GetCommentCountBySharedEventAsync(se.Id);
            dtos.Add(new SharedEventSummaryDto
            {
                SharedEventId = se.Id,
                Title = se.Title,
                Description = se.Description,
                Points = se.Points,
                IsClosed = se.IsClosed,
                ClosesAt = se.ClosesAt,
                CreatedAt = se.CreatedAt,
                CreatedByUserId = se.CreatedByUserId,
                CreatedByUserName = se.CreatedByUser?.Name ?? string.Empty,
                ParticipantCount = se.Participants.Count,
                HasCurrentUserJoined = se.Participants.Any(p => p.UserId == userId),
                CommentCount = commentCount
            });
        }

        return new ListGroupSharedEventsResponse
        {
            SharedEvents = dtos,
            HasMore = pagedSharedEvents.HasMore,
            NextCursor = pagedSharedEvents.NextCursor
        };
    }
}
