using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class ListGroupSharedEventsRequest
{
    public Guid GroupId { get; set; }
}

public class ListGroupSharedEventsResponse
{
    public List<SharedEventSummaryDto> SharedEvents { get; set; } = new();
}

public class SharedEventSummaryDto
{
    public Guid SharedEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
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

    public ListGroupSharedEventsHandler(
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ListGroupSharedEventsResponse> HandleAsync(ListGroupSharedEventsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var sharedEvents = await _sharedEventRepository.GetByGroupAsync(request.GroupId);

        var dtos = sharedEvents.Select(se => new SharedEventSummaryDto
        {
            SharedEventId = se.Id,
            Title = se.Title,
            Description = se.Description,
            Points = se.Points,
            IsClosed = se.IsClosed,
            CreatedAt = se.CreatedAt,
            CreatedByUserId = se.CreatedByUserId,
            CreatedByUserName = se.CreatedByUser?.Name ?? string.Empty,
            ParticipantCount = se.Participants.Count
        }).ToList();

        return new ListGroupSharedEventsResponse { SharedEvents = dtos };
    }
}
