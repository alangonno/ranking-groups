using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

namespace backend.src.Common.Rules;

public static class EventApprovalRules
{
    public static void ValidateCanVote(EventApproval approval, Event @event, User voter, IEnumerable<GroupMember> groupMembers)
    {
        if (approval.UserId == @event.AffectedUserId)
        {
            throw new BusinessRuleException(
                "affected_user_cannot_vote",
                "O usuário afetado pelo evento não pode votar na aprovação."
            );
        }

        var isMember = groupMembers.Any(gm => gm.UserId == voter.Id);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "non_member_cannot_vote",
                "Apenas membros do grupo podem votar na aprovação do evento."
            );
        }
    }

    public static void ValidateNoDuplicateVote(Guid userId, Guid eventId, IEnumerable<EventApproval> existingApprovals)
    {
        var alreadyVoted = existingApprovals.Any(a => a.UserId == userId && a.EventId == eventId);
        if (alreadyVoted)
        {
            throw new BusinessRuleException(
                "duplicate_vote_not_allowed",
                "O usuário já votou neste evento."
            );
        }
    }

    public static void ValidateEventIsPending(EventStatus status)
    {
        if (status != EventStatus.Pending)
        {
            throw new BusinessRuleException(
                "event_not_pending",
                "Só é possível votar em eventos com status Pendente."
            );
        }
    }

    public static void ValidateApprovalQuorum(int approvalCount, int totalMembers)
    {
        var required = (int)Math.Ceiling(totalMembers / 3.0);

        if (approvalCount < required)
        {
            throw new BusinessRuleException(
                "insufficient_approval_quorum",
                $"É necessário pelo menos {required} aprovação(ões). Atual: {approvalCount}."
            );
        }
    }
}
