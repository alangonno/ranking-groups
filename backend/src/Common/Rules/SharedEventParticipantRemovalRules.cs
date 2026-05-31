using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

namespace backend.src.Common.Rules;

public static class SharedEventParticipantRemovalRules
{
    public static void ValidateCanInitiateRemoval(SharedEventParticipant participant, Guid userId, IEnumerable<GroupMember> groupMembers)
    {
        if (participant.IsPendingRemoval)
        {
            throw new BusinessRuleException(
                "participant_already_pending_removal",
                "Este participante já possui uma votação de remoção em andamento."
            );
        }

        var isMember = groupMembers.Any(gm => gm.UserId == userId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "non_member_cannot_initiate_removal",
                "Apenas membros do grupo podem solicitar remoção de participantes."
            );
        }
    }

    public static void ValidateCanVoteRemoval(Guid userId, Guid affectedUserId, IEnumerable<SharedEventParticipantRemovalVote> existingVotes, IEnumerable<GroupMember> groupMembers)
    {
        if (userId == affectedUserId)
        {
            throw new BusinessRuleException(
                "affected_user_cannot_vote_removal",
                "O usuário afetado não pode votar na própria remoção."
            );
        }

        var isMember = groupMembers.Any(gm => gm.UserId == userId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "non_member_cannot_vote_removal",
                "Apenas membros do grupo podem votar na remoção."
            );
        }

        var alreadyVoted = existingVotes.Any(v => v.UserId == userId);
        if (alreadyVoted)
        {
            throw new BusinessRuleException(
                "duplicate_vote_not_allowed",
                "O usuário já votou nesta remoção."
            );
        }
    }

    public static void ValidateParticipantIsPendingRemoval(bool isPendingRemoval)
    {
        if (!isPendingRemoval)
        {
            throw new BusinessRuleException(
                "participant_not_pending_removal",
                "Este participante não está em votação de remoção."
            );
        }
    }

    public static void ValidateVoteDeadline(DateTime? deadline)
    {
        if (deadline.HasValue && DateTime.UtcNow > deadline.Value)
        {
            throw new BusinessRuleException(
                "removal_vote_deadline_expired",
                "O prazo para votação de remoção expirou."
            );
        }
    }

    public static int CalculateQuorum(int totalMembers)
    {
        return (int)Math.Ceiling(totalMembers / 3.0);
    }

    public static RemovalResolution ResolveExpiredRemovalVote(IEnumerable<GroupMember> groupMembers, IEnumerable<SharedEventParticipantRemovalVote> existingVotes)
    {
        var totalMembers = groupMembers.Count();
        var quorum = CalculateQuorum(totalMembers);

        var votedUserIds = existingVotes
            .Select(v => v.UserId)
            .ToHashSet();

        var removeCount = existingVotes.Count(v => v.VoteType == EventVoteType.Remove);
        var keepCount = existingVotes.Count(v => v.VoteType == EventVoteType.Keep);

        // Não-votantes = Keep
        var nonVoters = groupMembers.Where(m => !votedUserIds.Contains(m.UserId)).ToList();
        keepCount += nonVoters.Count;

        if (removeCount >= quorum && removeCount > keepCount)
        {
            return RemovalResolution.Remove;
        }

        // Keep sempre vence em caso de empate ou quorum não atingido
        return RemovalResolution.Keep;
    }
}
