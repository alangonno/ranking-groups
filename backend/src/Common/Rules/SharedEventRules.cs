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

    public static void ValidateCanClose(bool isClosed)
    {
        if (isClosed)
        {
            throw new BusinessRuleException(
                "event_already_closed",
                "O evento compartilhado já está fechado."
            );
        }
    }

    public static void ValidateCanRemoveParticipation(bool isClosed)
    {
        if (isClosed)
        {
            throw new BusinessRuleException(
                "cannot_leave_closed_event",
                "Não é possível remover participação de um evento compartilhado fechado."
            );
        }
    }

    public static void ValidateUserCanEditSharedEvent(Guid userId, Guid creatorId, IEnumerable<GroupMember> groupMembers)
    {
        if (userId == creatorId)
        {
            return;
        }

        var userRole = groupMembers.FirstOrDefault(gm => gm.UserId == userId)?.Role;
        if (userRole == GroupRole.Admin || userRole == GroupRole.Owner)
        {
            return;
        }

        throw new BusinessRuleException(
            "not_authorized_to_edit_shared_event",
            "Apenas o criador, admin ou owner pode editar este evento compartilhado."
        );
    }

    public static void ValidateUserCanCloseSharedEvent(Guid userId, Guid creatorId, IEnumerable<GroupMember> groupMembers)
    {
        if (userId == creatorId)
        {
            return;
        }

        var userRole = groupMembers.FirstOrDefault(gm => gm.UserId == userId)?.Role;
        if (userRole == GroupRole.Admin || userRole == GroupRole.Owner)
        {
            return;
        }

        throw new BusinessRuleException(
            "not_authorized_to_close_shared_event",
            "Apenas o criador, admin ou owner pode fechar este evento compartilhado."
        );
    }
}
