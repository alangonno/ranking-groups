using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class CreateSharedEventRequest
{
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime? ClosesAt { get; set; }
    public string? ImageUrl { get; set; }
    public List<Guid> ParticipantUserIds { get; set; } = new();
}

public class CreateSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
}

public interface ICreateSharedEventHandler
{
    Task<CreateSharedEventResponse> HandleAsync(CreateSharedEventRequest request, CancellationToken ct);
}

public class CreateSharedEventHandler : ICreateSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ISupabaseStorageService _storageService;
    private readonly AppDbContext _context;

    public CreateSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        INotificationRepository notificationRepository,
        ISupabaseStorageService storageService,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _notificationRepository = notificationRepository;
        _storageService = storageService;
        _context = context;
    }

    public async Task<CreateSharedEventResponse> HandleAsync(CreateSharedEventRequest request, CancellationToken ct)
    {
        CreateSharedEventRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        SharedEventRules.ValidatePoints(request.Points);

        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        if (group == null)
        {
            throw new BusinessRuleException("group_not_found", "Grupo não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var participantUserIds = NormalizeParticipantUserIds(request.ParticipantUserIds);
        SharedEventRules.ValidateParticipantsBelongToGroup(participantUserIds, members);

        var sharedEvent = new SharedEvent
        {
            GroupId = request.GroupId,
            CreatedByUserId = userId,
            Title = request.Title,
            Description = request.Description,
            Points = request.Points,
            IsClosed = false,
            ClosesAt = request.ClosesAt,
            ImageUrl = request.ImageUrl
        };

        _sharedEventRepository.Add(sharedEvent);
        await _context.SaveChangesAsync(ct);

        foreach (var participantUserId in participantUserIds)
        {
            _participantRepository.Add(new SharedEventParticipant
            {
                SharedEventId = sharedEvent.Id,
                UserId = participantUserId
            });

            var member = members.FirstOrDefault(m => m.UserId == participantUserId);
            if (member != null)
            {
                member.CurrentScore += sharedEvent.Points;
                _groupMemberRepository.Update(member);
            }
        }

        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventCreated(sharedEvent, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        var notifications = NotificationBuilder.BuildNotifications(auditLog, members, null, sharedEvent);
        _notificationRepository.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        return new CreateSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            Title = sharedEvent.Title,
            Description = sharedEvent.Description,
            Points = sharedEvent.Points,
            IsClosed = sharedEvent.IsClosed,
            CreatedAt = sharedEvent.CreatedAt,
            ImageUrl = _storageService.GetPublicUrlFromPath(sharedEvent.ImageUrl)
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

public static class CreateSharedEventRequestValidator
{
    public static void Validate(CreateSharedEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BusinessRuleException("title_required", "O título do evento é obrigatório.");
        }

        if (request.GroupId == Guid.Empty)
        {
            throw new BusinessRuleException("group_id_required", "O ID do grupo é obrigatório.");
        }
    }

}
