using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
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
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetEventHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
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
            Approvals = @event.Approvals.Select(a => new EventApprovalDto
            {
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                VoteType = a.VoteType.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }
}
