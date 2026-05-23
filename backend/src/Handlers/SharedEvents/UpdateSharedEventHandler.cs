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
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public UpdateSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
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

        sharedEvent.Title = request.Title;
        sharedEvent.Description = request.Description;
        sharedEvent.Points = request.Points;

        var oldPoints = sharedEvent.Points;

        _sharedEventRepository.Update(sharedEvent);
        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventUpdated(sharedEvent, oldPoints, userId);
        _auditLogRepository.Add(auditLog);
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
