using backend.src.Common.Enums;
using backend.src.Common.Exceptions;
using backend.src.Entities;

namespace backend.src.Common.Rules;

public static class EventRules
{
    public static void ValidateInitialStatus(EventType type, EventStatus status)
    {
        var expected = type == EventType.Negative ? EventStatus.Pending : EventStatus.Approved;

        if (status != expected)
        {
            throw new BusinessRuleException(
                "invalid_initial_status",
                $"Eventos do tipo {type} devem iniciar com status {expected}."
            );
        }
    }

    public static void ValidatePoints(int points)
    {
        if (points == 0)
        {
            throw new BusinessRuleException(
                "score_zero_not_allowed",
                "A pontuação do evento não pode ser zero."
            );
        }
    }

    public static void ValidateCanEdit(EventStatus currentStatus)
    {
        if (currentStatus == EventStatus.Approved)
        {
            throw new BusinessRuleException(
                "approved_event_cannot_be_edited",
                "Eventos aprovados não podem ser editados."
            );
        }
    }

    public static void ValidateAffectedUserIsNotCreator(Guid affectedUserId, Guid creatorUserId)
    {
        if (affectedUserId == creatorUserId)
        {
            throw new BusinessRuleException(
                "affected_user_is_creator",
                "O usuário afetado não pode ser o mesmo que o criador do evento."
            );
        }
    }
}
