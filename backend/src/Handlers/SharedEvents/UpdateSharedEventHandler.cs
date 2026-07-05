using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class UpdateSharedEventRequest
{
    public Guid SharedEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<Guid> ParticipantUserIds { get; set; } = new();
}

public class UpdateSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IUpdateSharedEventHandler
{
    Task<UpdateSharedEventResponse> HandleAsync(UpdateSharedEventRequest request, CancellationToken ct);
}

public class UpdateSharedEventHandler : IUpdateSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;

    public UpdateSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _context = context;
    }

    public async Task<UpdateSharedEventResponse> HandleAsync(UpdateSharedEventRequest request, CancellationToken ct)
    {
        UpdateSharedEventRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId);
        if (sharedEvent == null)
        {
            throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members);

        SharedEventRules.ValidateCanClose(sharedEvent.IsClosed);
        SharedEventRules.ValidateUserCanEditSharedEvent(userId, sharedEvent.CreatedByUserId, members);
        SharedEventRules.ValidatePoints(request.Points);

        var participantUserIds = NormalizeParticipantUserIds(request.ParticipantUserIds);
        SharedEventRules.ValidateParticipantsBelongToGroup(participantUserIds, members);

        var oldPoints = sharedEvent.Points;
        var currentParticipantUserIds = sharedEvent.Participants
            .Select(participant => participant.UserId)
            .ToHashSet();
        var requestedParticipantUserIds = participantUserIds.ToHashSet();

        sharedEvent.Title = request.Title;
        sharedEvent.Description = request.Description;
        sharedEvent.Points = request.Points;

        var removedParticipants = sharedEvent.Participants
            .Where(participant => !requestedParticipantUserIds.Contains(participant.UserId))
            .ToList();

        foreach (var participant in removedParticipants)
        {
            _participantRepository.Remove(participant);

            var member = members.FirstOrDefault(m => m.UserId == participant.UserId);
            if (member != null)
            {
                member.CurrentScore -= oldPoints;
                _groupMemberRepository.Update(member);
            }
        }

        foreach (var participantUserId in participantUserIds.Where(requestedId => !currentParticipantUserIds.Contains(requestedId)))
        {
            _participantRepository.Add(new SharedEventParticipant
            {
                SharedEventId = sharedEvent.Id,
                UserId = participantUserId
            });

            var member = members.FirstOrDefault(m => m.UserId == participantUserId);
            if (member != null)
            {
                member.CurrentScore += request.Points;
                _groupMemberRepository.Update(member);
            }
        }

        var delta = request.Points - oldPoints;
        if (delta != 0)
        {
            foreach (var participantUserId in participantUserIds.Where(requestedId => currentParticipantUserIds.Contains(requestedId)))
            {
                var member = members.FirstOrDefault(m => m.UserId == participantUserId);
                if (member != null)
                {
                    member.CurrentScore += delta;
                    _groupMemberRepository.Update(member);
                }
            }
        }

        _sharedEventRepository.Update(sharedEvent);
        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventUpdated(sharedEvent, oldPoints, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        var notifications = NotificationBuilder.BuildNotifications(auditLog, members, null, sharedEvent);
        _notificationRepository.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        return new UpdateSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            Title = sharedEvent.Title,
            Description = sharedEvent.Description,
            Points = sharedEvent.Points,
            IsClosed = sharedEvent.IsClosed,
            UpdatedAt = sharedEvent.UpdatedAt
        };
    }

    private static List<Guid> NormalizeParticipantUserIds(IEnumerable<Guid>? participantUserIds)
    {
        return participantUserIds?
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList()
            ?? new List<Guid>();
    }
}

public static class UpdateSharedEventRequestValidator
{
    public static void Validate(UpdateSharedEventRequest request)
    {
        if (request.SharedEventId == Guid.Empty)
        {
            throw new BusinessRuleException("shared_event_id_required", "O ID do evento é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BusinessRuleException("title_required", "O título do evento é obrigatório.");
        }
    }
}
