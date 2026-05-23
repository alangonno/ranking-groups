using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class DeleteSharedEventRequest
{
    public Guid SharedEventId { get; set; }
}

public class DeleteSharedEventResponse
{
    public bool Success { get; set; }
}

public interface IDeleteSharedEventHandler
{
    Task<DeleteSharedEventResponse> HandleAsync(DeleteSharedEventRequest request, CancellationToken ct);
}

public class DeleteSharedEventHandler : IDeleteSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public DeleteSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<DeleteSharedEventResponse> HandleAsync(DeleteSharedEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId);
        if (sharedEvent == null)
        {
            throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members);
        SharedEventRules.ValidateUserCanEditSharedEvent(userId, sharedEvent.CreatedByUserId, members);

        // Reverter pontos de todos os participantes
        var participants = await _participantRepository.GetBySharedEventAsync(request.SharedEventId);
        foreach (var participant in participants)
        {
            var member = await _groupMemberRepository.GetByGroupAndUserAsync(sharedEvent.GroupId, participant.UserId);
            if (member != null)
            {
                member.CurrentScore -= sharedEvent.Points;
                _groupMemberRepository.Update(member);
            }
        }

        var participantsCount = participants.Count();

        _sharedEventRepository.Remove(sharedEvent);
        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventDeleted(sharedEvent, participantsCount, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return new DeleteSharedEventResponse { Success = true };
    }
}
