using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Events;

public class CreateEventRequest
{
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public EventType Type { get; set; }
    public Guid AffectedUserId { get; set; }
}

public class CreateEventResponse
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface ICreateEventHandler
{
    Task<CreateEventResponse> HandleAsync(CreateEventRequest request, CancellationToken ct);
}

public class CreateEventHandler : ICreateEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public CreateEventHandler(
        IEventRepository eventRepository,
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _eventRepository = eventRepository;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<CreateEventResponse> HandleAsync(CreateEventRequest request, CancellationToken ct)
    {
        CreateEventRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        EventRules.ValidatePoints(request.Points);

        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        if (group == null)
        {
            throw new BusinessRuleException("group_not_found", "Grupo não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var affectedIsMember = members.Any(m => m.UserId == request.AffectedUserId);
        if (!affectedIsMember)
        {
            throw new BusinessRuleException("affected_user_not_member", "O usuário afetado deve ser membro do grupo.");
        }

        var isSelfImposed = request.AffectedUserId == userId;
        var status = (request.Type == EventType.Negative && !isSelfImposed)
            ? EventStatus.Pending
            : EventStatus.Approved;

        if (!isSelfImposed)
            EventRules.ValidateInitialStatus(request.Type, status);

        var @event = new Event
        {
            GroupId = request.GroupId,
            CreatedByUserId = userId,
            AffectedUserId = request.AffectedUserId,
            Title = request.Title,
            Description = request.Description,
            Points = request.Points,
            Type = request.Type,
            Status = status
        };

        _eventRepository.Add(@event);
        await _context.SaveChangesAsync(ct);

        if (status == EventStatus.Approved)
        {
            await UpdateAffectedUserScoreAsync(request.GroupId, request.AffectedUserId, request.Type, request.Points);
            await _context.SaveChangesAsync(ct);
        }

        var auditLog = AuditLogBuilder.EventCreated(@event, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return new CreateEventResponse
        {
            EventId = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            Points = @event.Points,
            Type = @event.Type.ToString(),
            Status = @event.Status.ToString(),
            CreatedAt = @event.CreatedAt
        };
    }

    private async Task UpdateAffectedUserScoreAsync(Guid groupId, Guid affectedUserId, EventType type, int points)
    {
        var member = await _groupMemberRepository.GetByGroupAndUserAsync(groupId, affectedUserId);
        if (member != null)
        {
            member.CurrentScore += type == EventType.Negative ? -points : points;
            _groupMemberRepository.Update(member);
        }
    }
}

public static class CreateEventRequestValidator
{
    public static void Validate(CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BusinessRuleException("title_required", "O título do evento é obrigatório.");
        }

        if (request.GroupId == Guid.Empty)
        {
            throw new BusinessRuleException("group_id_required", "O ID do grupo é obrigatório.");
        }

        if (request.AffectedUserId == Guid.Empty)
        {
            throw new BusinessRuleException("affected_user_id_required", "O ID do usuário afetado é obrigatório.");
        }
    }
}
