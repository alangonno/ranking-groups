using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class UpdateEventRequest
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class UpdateEventResponse
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public interface IUpdateEventHandler
{
    Task<UpdateEventResponse> HandleAsync(UpdateEventRequest request, CancellationToken ct);
}

public class UpdateEventHandler : IUpdateEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public UpdateEventHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<UpdateEventResponse> HandleAsync(UpdateEventRequest request, CancellationToken ct)
    {
        UpdateEventRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, @event.GroupId, members);

        EventRules.ValidateCanEdit(@event.Status);
        EventRules.ValidateAffectedUserCannotModify(@event.AffectedUserId, userId, @event.Type);

        if (@event.CreatedByUserId != userId)
        {
            throw new BusinessRuleException("not_event_creator", "Apenas o criador do evento pode editá-lo.");
        }

        EventRules.ValidatePoints(request.Points);

        if (@event.Status == EventStatus.Approved)
        {
            var oldImpact = @event.Type == EventType.Negative ? -@event.Points : @event.Points;
            var newImpact = @event.Type == EventType.Negative ? -request.Points : request.Points;
            var delta = newImpact - oldImpact;

            if (delta != 0)
            {
                var member = await _groupMemberRepository.GetByGroupAndUserAsync(@event.GroupId, @event.AffectedUserId);
                if (member != null)
                {
                    member.CurrentScore += delta;
                    _groupMemberRepository.Update(member);
                }
            }
        }

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.Points = request.Points;

        var oldPoints = @event.Points;

        _eventRepository.Update(@event);
        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.EventUpdated(@event, oldPoints, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return new UpdateEventResponse
        {
            EventId = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            Points = @event.Points,
            Status = @event.Status.ToString(),
            UpdatedAt = @event.UpdatedAt
        };
    }
}

public static class UpdateEventRequestValidator
{
    public static void Validate(UpdateEventRequest request)
    {
        if (request.EventId == Guid.Empty)
        {
            throw new BusinessRuleException("event_id_required", "O ID do evento é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BusinessRuleException("title_required", "O título do evento é obrigatório.");
        }
    }
}
