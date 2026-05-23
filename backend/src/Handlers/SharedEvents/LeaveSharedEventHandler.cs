using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class LeaveSharedEventRequest
{
    public Guid SharedEventId { get; set; }
}

public class LeaveSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public Guid UserId { get; set; }
    public int ParticipantCount { get; set; }
}

public interface ILeaveSharedEventHandler
{
    Task<LeaveSharedEventResponse> HandleAsync(LeaveSharedEventRequest request, CancellationToken ct);
}

public class LeaveSharedEventHandler : ILeaveSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public LeaveSharedEventHandler(
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

    public async Task<LeaveSharedEventResponse> HandleAsync(LeaveSharedEventRequest request, CancellationToken ct)
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

        SharedEventRules.ValidateCanRemoveParticipation(sharedEvent.IsClosed);

        var participant = await _participantRepository.GetBySharedEventAndUserAsync(request.SharedEventId, userId);
        if (participant == null)
        {
            throw new BusinessRuleException("not_participating", "O usuário não está participando deste evento.");
        }

        _participantRepository.Remove(participant);

        var member = await _groupMemberRepository.GetByGroupAndUserAsync(sharedEvent.GroupId, userId);
        if (member != null)
        {
            member.CurrentScore -= sharedEvent.Points;
            _groupMemberRepository.Update(member);
        }

        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventLeft(sharedEvent, userId, member?.User?.Name ?? string.Empty, sharedEvent.Points);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        var remainingParticipants = await _participantRepository.GetBySharedEventAsync(request.SharedEventId);

        return new LeaveSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            UserId = userId,
            ParticipantCount = remainingParticipants.Count()
        };
    }
}
