using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class JoinSharedEventRequest
{
    public Guid SharedEventId { get; set; }
}

public class JoinSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public Guid UserId { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime JoinedAt { get; set; }
}

public interface IJoinSharedEventHandler
{
    Task<JoinSharedEventResponse> HandleAsync(JoinSharedEventRequest request, CancellationToken ct);
}

public class JoinSharedEventHandler : IJoinSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public JoinSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<JoinSharedEventResponse> HandleAsync(JoinSharedEventRequest request, CancellationToken ct)
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

        SharedEventRules.ValidateNotClosed(sharedEvent.IsClosed);

        var existingParticipants = await _participantRepository.GetBySharedEventAsync(request.SharedEventId);
        SharedEventRules.ValidateNoDuplicateParticipation(userId, request.SharedEventId, existingParticipants);

        var participant = new SharedEventParticipant
        {
            SharedEventId = request.SharedEventId,
            UserId = userId
        };

        _participantRepository.Add(participant);

        var member = await _groupMemberRepository.GetByGroupAndUserAsync(sharedEvent.GroupId, userId);
        if (member != null)
        {
            member.CurrentScore += sharedEvent.Points;
            _groupMemberRepository.Update(member);
        }

        await _context.SaveChangesAsync(ct);

        var updatedParticipants = await _participantRepository.GetBySharedEventAsync(request.SharedEventId);

        return new JoinSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            UserId = userId,
            ParticipantCount = updatedParticipants.Count(),
            JoinedAt = participant.CreatedAt
        };
    }
}
