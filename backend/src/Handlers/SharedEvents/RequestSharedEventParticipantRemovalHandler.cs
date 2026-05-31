using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class RequestSharedEventParticipantRemovalRequest
{
    public Guid SharedEventId { get; set; }
    public Guid ParticipantId { get; set; }
}

public class RequestSharedEventParticipantRemovalResponse
{
    public Guid SharedEventId { get; set; }
    public Guid ParticipantId { get; set; }
    public bool IsPendingRemoval { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public int QuorumRequired { get; set; }
    public bool RemovedImmediately { get; set; }
}

public interface IRequestSharedEventParticipantRemovalHandler
{
    Task<RequestSharedEventParticipantRemovalResponse> HandleAsync(RequestSharedEventParticipantRemovalRequest request, CancellationToken ct);
}

public class RequestSharedEventParticipantRemovalHandler : IRequestSharedEventParticipantRemovalHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly ISharedEventParticipantRemovalVoteRepository _voteRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public RequestSharedEventParticipantRemovalHandler(
        ISharedEventRepository sharedEventRepository,
        ISharedEventParticipantRepository participantRepository,
        ISharedEventParticipantRemovalVoteRepository voteRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _sharedEventRepository = sharedEventRepository;
        _participantRepository = participantRepository;
        _voteRepository = voteRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<RequestSharedEventParticipantRemovalResponse> HandleAsync(RequestSharedEventParticipantRemovalRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId);
        if (sharedEvent == null)
        {
            throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
        }

        var participant = await _participantRepository.GetBySharedEventAndUserAsync(request.SharedEventId, request.ParticipantId);
        if (participant == null)
        {
            throw new BusinessRuleException("participant_not_found", "Participante não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        SharedEventParticipantRemovalRules.ValidateCanInitiateRemoval(participant, userId, members);

        var totalMembers = members.Count();
        var quorum = SharedEventParticipantRemovalRules.CalculateQuorum(totalMembers);

        // Abre votação de remoção com prazo de 48h
        participant.IsPendingRemoval = true;
        participant.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
        _participantRepository.Update(participant);

        // Participante afetado auto-vota Keep
        _voteRepository.Add(new SharedEventParticipantRemovalVote
        {
            SharedEventId = sharedEvent.Id,
            ParticipantId = participant.Id,
            UserId = participant.UserId,
            VoteType = EventVoteType.Keep
        });

        // Iniciador vota Remove (se não for o participante afetado)
        if (userId != participant.UserId)
        {
            _voteRepository.Add(new SharedEventParticipantRemovalVote
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.Id,
                UserId = userId,
                VoteType = EventVoteType.Remove
            });
        }

        await _context.SaveChangesAsync(ct);

        var existingVotes = await _voteRepository.GetByParticipantAsync(participant.Id);
        var removeCount = existingVotes.Count(v => v.VoteType == EventVoteType.Remove);
        var keepCount = existingVotes.Count(v => v.VoteType == EventVoteType.Keep);

        // Verifica se quorum já foi atingido imediatamente
        if (removeCount >= quorum && removeCount > keepCount)
        {
            await RemoveParticipantImmediatelyAsync(sharedEvent, participant, userId, ct);

            return new RequestSharedEventParticipantRemovalResponse
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.UserId,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovedImmediately = true
            };
        }

        if (keepCount >= quorum && keepCount > removeCount)
        {
            participant.IsPendingRemoval = false;
            participant.RemovalVoteDeadline = null;
            _participantRepository.Update(participant);

            // Limpa votos
            var votesToRemove = existingVotes.ToList();
            _voteRepository.RemoveRange(votesToRemove);

            await _context.SaveChangesAsync(ct);

            return new RequestSharedEventParticipantRemovalResponse
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.UserId,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovedImmediately = false
            };
        }

        return new RequestSharedEventParticipantRemovalResponse
        {
            SharedEventId = sharedEvent.Id,
            ParticipantId = participant.UserId,
            IsPendingRemoval = true,
            RemoveCount = removeCount,
            KeepCount = keepCount,
            QuorumRequired = quorum,
            RemovedImmediately = false
        };
    }

    private async Task RemoveParticipantImmediatelyAsync(SharedEvent sharedEvent, SharedEventParticipant participant, Guid performedByUserId, CancellationToken ct)
    {
        var member = await _groupMemberRepository.GetByGroupAndUserAsync(sharedEvent.GroupId, participant.UserId);
        if (member != null)
        {
            member.CurrentScore -= sharedEvent.Points;
            _groupMemberRepository.Update(member);
        }

        _participantRepository.Remove(participant);

        // Limpa votos
        var votes = await _voteRepository.GetByParticipantAsync(participant.Id);
        _voteRepository.RemoveRange(votes);

        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.SharedEventLeft(sharedEvent, participant.UserId, member?.User?.Name ?? string.Empty, sharedEvent.Points);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);
    }
}
