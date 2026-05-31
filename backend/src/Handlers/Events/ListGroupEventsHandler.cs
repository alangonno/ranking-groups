using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
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
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
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

    public ListGroupEventsHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ListGroupEventsResponse> HandleAsync(ListGroupEventsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);

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
            RemoveCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Remove),
            KeepCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Keep)
        }).ToList();

        return new ListGroupEventsResponse { Events = dtos };
    }
}
