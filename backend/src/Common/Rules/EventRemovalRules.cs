using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

namespace backend.src.Common.Rules;

public static class EventRemovalRules
{
    public static void ValidateCanInitiateRemoval(Event @event, Guid userId, IEnumerable<GroupMember> groupMembers)
    {
        if (@event.Status != EventStatus.Approved)
        {
            throw new BusinessRuleException(
                "event_not_approved",
                "Só é possível solicitar remoção de eventos aprovados."
            );
        }

        if (@event.IsPendingRemoval)
        {
            throw new BusinessRuleException(
                "event_already_pending_removal",
                "Este evento já possui uma votação de remoção em andamento."
            );
        }

        var isMember = groupMembers.Any(gm => gm.UserId == userId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "non_member_cannot_initiate_removal",
                "Apenas membros do grupo podem solicitar remoção de eventos."
            );
        }
    }

    public static void ValidateCanVoteRemoval(Guid userId, Event @event, IEnumerable<EventApproval> existingApprovals, IEnumerable<GroupMember> groupMembers)
    {
        if (userId == @event.AffectedUserId)
        {
            throw new BusinessRuleException(
                "affected_user_cannot_vote_removal",
                "O usuário afetado pelo evento não pode votar na remoção."
            );
        }

        var isMember = groupMembers.Any(gm => gm.UserId == userId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "non_member_cannot_vote_removal",
                "Apenas membros do grupo podem votar na remoção do evento."
            );
        }

        var alreadyVoted = existingApprovals.Any(a => a.UserId == userId && a.EventId == @event.Id);
        if (alreadyVoted)
        {
            throw new BusinessRuleException(
                "duplicate_vote_not_allowed",
                "O usuário já votou neste evento."
            );
        }
    }

    public static void ValidateEventIsPendingRemoval(bool isPendingRemoval)
    {
        if (!isPendingRemoval)
        {
            throw new BusinessRuleException(
                "event_not_pending_removal",
                "Este evento não está em votação de remoção."
            );
        }
    }

    public static int CalculateQuorum(int totalMembers)
    {
        return (int)Math.Ceiling(totalMembers / 3.0);
    }

    public static void ValidateRemoveQuorum(int removeCount, int keepCount, int totalMembers)
    {
        var quorum = CalculateQuorum(totalMembers);

        if (removeCount >= quorum && removeCount > keepCount)
            return;

        if (keepCount >= quorum && keepCount > removeCount)
            return;

        throw new BusinessRuleException(
            "removal_quorum_not_reached",
            $"Nenhum lado atingiu a maioria. Remoção: {removeCount}/{quorum}, Manutenção: {keepCount}/{quorum}."
        );
    }
}
