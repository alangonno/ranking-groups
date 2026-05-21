using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

namespace backend.src.Common.Rules;

public static class SharedEventRules
{
    public static void ValidatePositiveOnly(EventType type)
    {
        if (type == EventType.Negative)
        {
            throw new BusinessRuleException(
                "shared_event_must_be_positive",
                "Eventos compartilhados devem ser apenas positivos."
            );
        }
    }

    public static void ValidatePoints(int points)
    {
        if (points <= 0)
        {
            throw new BusinessRuleException(
                "shared_event_points_must_be_positive",
                "A pontuação de um evento compartilhado deve ser maior que zero."
            );
        }
    }

    public static void ValidateGroupMembership(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        var isMember = groupMembers.Any(gm => gm.UserId == userId && gm.GroupId == groupId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "only_group_members_can_participate",
                "Apenas membros do grupo podem participar de eventos compartilhados."
            );
        }
    }

    public static void ValidateNotClosed(bool isClosed)
    {
        if (isClosed)
        {
            throw new BusinessRuleException(
                "event_is_closed",
                "Não é possível participar de um evento compartilhado fechado."
            );
        }
    }

    public static void ValidateNoDuplicateParticipation(Guid userId, Guid sharedEventId, IEnumerable<SharedEventParticipant> existingParticipants)
    {
        var alreadyParticipating = existingParticipants.Any(p => p.UserId == userId && p.SharedEventId == sharedEventId);
        if (alreadyParticipating)
        {
            throw new BusinessRuleException(
                "duplicate_participation_not_allowed",
                "O usuário já está participando deste evento compartilhado."
            );
        }
    }
}
