using backend.src.Common.Exceptions;
using backend.src.Entities;

namespace backend.src.Common.Rules;

public static class CommentRules
{
    public static void ValidateContentNotEmpty(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new BusinessRuleException(
                "comment_content_required",
                "O conteúdo do comentário é obrigatório."
            );
        }
    }

    public static void ValidateEventOrSharedEventProvided(Guid? eventId, Guid? sharedEventId)
    {
        var hasEventId = eventId.HasValue && eventId.Value != Guid.Empty;
        var hasSharedEventId = sharedEventId.HasValue && sharedEventId.Value != Guid.Empty;

        if (!hasEventId && !hasSharedEventId)
        {
            throw new BusinessRuleException(
                "comment_target_required",
                "O comentário deve estar associado a um evento ou evento compartilhado."
            );
        }

        if (hasEventId && hasSharedEventId)
        {
            throw new BusinessRuleException(
                "comment_single_target_only",
                "O comentário não pode estar associado a um evento e a um evento compartilhado simultaneamente."
            );
        }
    }

    public static void ValidateParentCommentBelongsToSamePost(
        Comment parentComment,
        Guid? eventId,
        Guid? sharedEventId)
    {
        if (parentComment.EventId.HasValue)
        {
            if (parentComment.EventId != eventId)
            {
                throw new BusinessRuleException(
                    "comment_parent_mismatch",
                    "A resposta deve pertencer ao mesmo evento do comentário raiz."
                );
            }
        }

        if (parentComment.SharedEventId.HasValue)
        {
            if (parentComment.SharedEventId != sharedEventId)
            {
                throw new BusinessRuleException(
                    "comment_parent_mismatch",
                    "A resposta deve pertencer ao mesmo evento compartilhado do comentário raiz."
                );
            }
        }
    }
}
