using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class VoteSharedEventParticipantRemovalRequest
{
    public Guid SharedEventId { get; set; }
    public Guid ParticipantId { get; set; }
    public EventVoteType VoteType { get; set; }
}

public class VoteSharedEventParticipantRemovalResponse
{
    public Guid SharedEventId { get; set; }
    public Guid ParticipantId { get; set; }
    public bool IsPendingRemoval { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public int QuorumRequired { get; set; }
    public bool RemovalResolved { get; set; }
    public bool ParticipantRemoved { get; set; }
}

public interface IVoteSharedEventParticipantRemovalHandler
{
    Task<VoteSharedEventParticipantRemovalResponse> HandleAsync(VoteSharedEventParticipantRemovalRequest request, CancellationToken ct);
}

public class VoteSharedEventParticipantRemovalHandler : IVoteSharedEventParticipantRemovalHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ISharedEventParticipantRepository _participantRepository;
    private readonly ISharedEventParticipantRemovalVoteRepository _voteRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public VoteSharedEventParticipantRemovalHandler(
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

    public async Task<VoteSharedEventParticipantRemovalResponse> HandleAsync(VoteSharedEventParticipantRemovalRequest request, CancellationToken ct)
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
        var existingVotes = await _voteRepository.GetByParticipantAsync(participant.Id);

        // Resolve votação expirada antes de aceitar novo voto
        if (participant.IsPendingRemoval && participant.RemovalVoteDeadline.HasValue && DateTime.UtcNow > participant.RemovalVoteDeadline.Value)
        {
            var resolution = SharedEventParticipantRemovalRules.ResolveExpiredRemovalVote(members, existingVotes);
            return await ResolveExpiredRemovalAsync(sharedEvent, participant, userId, resolution, members, existingVotes, ct);
        }

        SharedEventParticipantRemovalRules.ValidateParticipantIsPendingRemoval(participant.IsPendingRemoval);
        SharedEventParticipantRemovalRules.ValidateVoteDeadline(participant.RemovalVoteDeadline);
        SharedEventParticipantRemovalRules.ValidateCanVoteRemoval(userId, participant.UserId, existingVotes, members);

        var vote = new SharedEventParticipantRemovalVote
        {
            SharedEventId = sharedEvent.Id,
            ParticipantId = participant.Id,
            UserId = userId,
            VoteType = request.VoteType
        };

        _voteRepository.Add(vote);
        await _context.SaveChangesAsync(ct);

        var updatedVotes = await _voteRepository.GetByParticipantAsync(participant.Id);
        var removeCount = updatedVotes.Count(v => v.VoteType == EventVoteType.Remove);
        var keepCount = updatedVotes.Count(v => v.VoteType == EventVoteType.Keep);
        var totalMembers = members.Count();
        var quorum = SharedEventParticipantRemovalRules.CalculateQuorum(totalMembers);
        var removalResolved = false;

        if (removeCount >= quorum && removeCount > keepCount)
        {
            await RemoveParticipantAsync(sharedEvent, participant, userId, ct);
            removalResolved = true;

            return new VoteSharedEventParticipantRemovalResponse
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.UserId,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovalResolved = true,
                ParticipantRemoved = true
            };
        }

        if (keepCount >= quorum && keepCount > removeCount)
        {
            participant.IsPendingRemoval = false;
            participant.RemovalVoteDeadline = null;
            _participantRepository.Update(participant);

            // Limpa votos
            _voteRepository.RemoveRange(updatedVotes);

            await _context.SaveChangesAsync(ct);

            removalResolved = true;
        }

        return new VoteSharedEventParticipantRemovalResponse
        {
            SharedEventId = sharedEvent.Id,
            ParticipantId = participant.UserId,
            IsPendingRemoval = participant.IsPendingRemoval,
            RemoveCount = removeCount,
            KeepCount = keepCount,
            QuorumRequired = quorum,
            RemovalResolved = removalResolved,
            ParticipantRemoved = false
        };
    }

    private async Task<VoteSharedEventParticipantRemovalResponse> ResolveExpiredRemovalAsync(
        SharedEvent sharedEvent, SharedEventParticipant participant, Guid userId,
        RemovalResolution resolution, IEnumerable<GroupMember> members,
        IEnumerable<SharedEventParticipantRemovalVote> existingVotes, CancellationToken ct)
    {
        var totalMembers = members.Count();
        var quorum = SharedEventParticipantRemovalRules.CalculateQuorum(totalMembers);
        var removeCount = existingVotes.Count(v => v.VoteType == EventVoteType.Remove);
        var keepCount = existingVotes.Count(v => v.VoteType == EventVoteType.Keep);

        // Adiciona não-votantes como Keep para auditoria
        var votedUserIds = existingVotes.Select(v => v.UserId).ToHashSet();
        var nonVoters = members.Where(m => !votedUserIds.Contains(m.UserId)).ToList();
        keepCount += nonVoters.Count;

        foreach (var nonVoter in nonVoters)
        {
            _voteRepository.Add(new SharedEventParticipantRemovalVote
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.Id,
                UserId = nonVoter.UserId,
                VoteType = EventVoteType.Keep
            });
        }

        if (resolution == RemovalResolution.Remove)
        {
            await RemoveParticipantAsync(sharedEvent, participant, userId, ct);

            return new VoteSharedEventParticipantRemovalResponse
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.UserId,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovalResolved = true,
                ParticipantRemoved = true
            };
        }
        else
        {
            participant.IsPendingRemoval = false;
            participant.RemovalVoteDeadline = null;
            _participantRepository.Update(participant);

            // Limpa votos
            _voteRepository.RemoveRange(existingVotes);

            await _context.SaveChangesAsync(ct);

            return new VoteSharedEventParticipantRemovalResponse
            {
                SharedEventId = sharedEvent.Id,
                ParticipantId = participant.UserId,
                IsPendingRemoval = false,
                RemoveCount = removeCount,
                KeepCount = keepCount,
                QuorumRequired = quorum,
                RemovalResolved = true,
                ParticipantRemoved = false
            };
        }
    }

    private async Task RemoveParticipantAsync(SharedEvent sharedEvent, SharedEventParticipant participant, Guid performedByUserId, CancellationToken ct)
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
