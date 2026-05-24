using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;

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
        if (points <= 0)
        {
            throw new BusinessRuleException(
                "score_must_be_positive",
                "A pontuação do evento deve ser maior que zero."
            );
        }
    }

    public static void ValidateAffectedUserCannotModify(Guid affectedUserId, Guid modifierUserId, EventType type)
    {
        if (type == EventType.Negative && affectedUserId == modifierUserId)
        {
            throw new BusinessRuleException(
                "affected_user_cannot_modify_negative_event",
                "O usuário afetado não pode editar ou excluir um evento negativo relacionado a ele."
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

}
