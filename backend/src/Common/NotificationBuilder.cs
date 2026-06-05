using backend.src.Entities;
using System.Text.Json;

namespace backend.src.Common;

public static class NotificationBuilder
{
    public static List<Notification> BuildNotifications(AuditLog auditLog, IEnumerable<GroupMember> groupMembers, Event? @event = null, SharedEvent? sharedEvent = null)
    {
        var notifications = new List<Notification>();
        var performedByUserId = auditLog.PerformedByUserId;
        var values = auditLog.NewValues;
        var action = auditLog.Action;
        var groupId = @event?.GroupId ?? sharedEvent?.GroupId ?? Guid.Empty;

        if (groupId == Guid.Empty)
            return notifications;

        var newValues = values != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(values) : null;
        var performedByName = GetValue(newValues, "performedBy");
        var eventName = GetValue(newValues, "eventName");
        var points = GetValue(newValues, "points");
        var type = GetValue(newValues, "type");
        var affectedUser = GetValue(newValues, "affectedUser");
        var participant = GetValue(newValues, "participant");
        var groupName = GetValue(newValues, "groupName");
        var content = GetValue(newValues, "content");
        var parentCommentId = GetValue(newValues, "parentCommentId");

        var memberIds = groupMembers.Select(m => m.UserId).ToList();

        switch (action)
        {
            case "event_created":
                var eventCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var eventAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var eventTitle = eventName ?? "Evento";
                var eventPoints = points ?? "0";
                var eventType = type ?? "";

                // Notificar criador
                if (eventCreatorId != Guid.Empty && eventCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(eventCreatorId, groupId, "Evento Criado",
                        $"Você criou o evento '{eventTitle}' com {eventPoints} pontos.",
                        action, @event?.Id, null));
                }

                // Notificar afetado
                if (eventAffectedId != Guid.Empty && eventAffectedId != performedByUserId && eventAffectedId != eventCreatorId)
                {
                    var sign = eventType == "Negative" ? "-" : "+";
                    notifications.Add(CreateNotification(eventAffectedId, groupId, "Novo Evento Sobre Você",
                        $"{performedByName} criou o evento '{eventTitle}' com {sign}{eventPoints} pontos.",
                        action, @event?.Id, null));
                }
                break;

            case "event_updated":
                var updatedCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var updatedAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var updatedTitle = eventName ?? "Evento";
                var oldPoints = GetValue(newValues, "oldPoints");
                var newPoints = GetValue(newValues, "newPoints");

                if (updatedCreatorId != Guid.Empty && updatedCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(updatedCreatorId, groupId, "Evento Atualizado",
                        $"O evento '{updatedTitle}' teve seus pontos alterados de {oldPoints} para {newPoints}.",
                        action, @event?.Id, null));
                }

                if (updatedAffectedId != Guid.Empty && updatedAffectedId != performedByUserId && updatedAffectedId != updatedCreatorId)
                {
                    notifications.Add(CreateNotification(updatedAffectedId, groupId, "Evento Atualizado",
                        $"O evento '{updatedTitle}' teve seus pontos alterados de {oldPoints} para {newPoints}.",
                        action, @event?.Id, null));
                }
                break;

            case "event_deleted":
                var deletedAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var deletedCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var deletedTitle = eventName ?? "Evento";
                var revertedPoints = GetValue(newValues, "revertedPoints");

                if (deletedCreatorId != Guid.Empty && deletedCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(deletedCreatorId, groupId, "Evento Deletado",
                        $"O evento '{deletedTitle}' foi deletado. Pontos revertidos: {revertedPoints}.",
                        action, @event?.Id, null));
                }

                if (deletedAffectedId != Guid.Empty && deletedAffectedId != performedByUserId && deletedAffectedId != deletedCreatorId)
                {
                    notifications.Add(CreateNotification(deletedAffectedId, groupId, "Evento Deletado",
                        $"O evento '{deletedTitle}' foi deletado. Pontos revertidos: {revertedPoints}.",
                        action, @event?.Id, null));
                }
                break;

            case "event_approved":
                var approvedCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var approvedAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var approvedTitle = eventName ?? "Evento";
                var appliedPoints = GetValue(newValues, "appliedPoints");
                var approvedType = type ?? "";

                if (approvedCreatorId != Guid.Empty && approvedCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(approvedCreatorId, groupId, "Evento Aprovado",
                        $"O evento '{approvedTitle}' foi aprovado com {appliedPoints} pontos.",
                        action, @event?.Id, null));
                }

                if (approvedAffectedId != Guid.Empty && approvedAffectedId != performedByUserId && approvedAffectedId != approvedCreatorId)
                {
                    var approvedSign = approvedType == "Negative" ? "-" : "+";
                    notifications.Add(CreateNotification(approvedAffectedId, groupId, "Evento Aprovado",
                        $"O evento '{approvedTitle}' foi aprovado com {approvedSign}{appliedPoints} pontos.",
                        action, @event?.Id, null));
                }
                break;

            case "event_rejected_deleted":
                var rejectedCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var rejectedAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var rejectedTitle = eventName ?? "Evento";

                if (rejectedCreatorId != Guid.Empty && rejectedCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(rejectedCreatorId, groupId, "Evento Rejeitado",
                        $"O evento '{rejectedTitle}' foi rejeitado e deletado.",
                        action, @event?.Id, null));
                }

                if (rejectedAffectedId != Guid.Empty && rejectedAffectedId != performedByUserId && rejectedAffectedId != rejectedCreatorId)
                {
                    notifications.Add(CreateNotification(rejectedAffectedId, groupId, "Evento Rejeitado",
                        $"O evento '{rejectedTitle}' foi rejeitado e deletado.",
                        action, @event?.Id, null));
                }
                break;

            case "event_removed_by_vote":
                var removedByVoteCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var removedByVoteAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var removedByVoteTitle = eventName ?? "Evento";
                var removedByVotePoints = GetValue(newValues, "revertedPoints");

                if (removedByVoteCreatorId != Guid.Empty && removedByVoteCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(removedByVoteCreatorId, groupId, "Evento Removido",
                        $"O evento '{removedByVoteTitle}' foi removido por votação. Pontos revertidos: {removedByVotePoints}.",
                        action, @event?.Id, null));
                }

                if (removedByVoteAffectedId != Guid.Empty && removedByVoteAffectedId != performedByUserId && removedByVoteAffectedId != removedByVoteCreatorId)
                {
                    notifications.Add(CreateNotification(removedByVoteAffectedId, groupId, "Evento Removido",
                        $"O evento '{removedByVoteTitle}' foi removido por votação. Pontos revertidos: {removedByVotePoints}.",
                        action, @event?.Id, null));
                }
                break;

            case "event_removal_initiated":
                var removalInitiatedCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var removalInitiatedAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var removalInitiatedTitle = eventName ?? "Evento";

                if (removalInitiatedCreatorId != Guid.Empty && removalInitiatedCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(removalInitiatedCreatorId, groupId, "Remoção Iniciada",
                        $"Uma votação para remover o evento '{removalInitiatedTitle}' foi iniciada.",
                        action, @event?.Id, null));
                }

                if (removalInitiatedAffectedId != Guid.Empty && removalInitiatedAffectedId != performedByUserId && removalInitiatedAffectedId != removalInitiatedCreatorId)
                {
                    notifications.Add(CreateNotification(removalInitiatedAffectedId, groupId, "Remoção Iniciada",
                        $"Uma votação para remover o evento '{removalInitiatedTitle}' foi iniciada.",
                        action, @event?.Id, null));
                }
                break;

            case "event_removal_cancelled":
                var removalCancelledCreatorId = @event?.CreatedByUserId ?? Guid.Empty;
                var removalCancelledAffectedId = @event?.AffectedUserId ?? Guid.Empty;
                var removalCancelledTitle = eventName ?? "Evento";

                if (removalCancelledCreatorId != Guid.Empty && removalCancelledCreatorId != performedByUserId)
                {
                    notifications.Add(CreateNotification(removalCancelledCreatorId, groupId, "Remoção Cancelada",
                        $"A votação para remover o evento '{removalCancelledTitle}' foi cancelada.",
                        action, @event?.Id, null));
                }

                if (removalCancelledAffectedId != Guid.Empty && removalCancelledAffectedId != performedByUserId && removalCancelledAffectedId != removalCancelledCreatorId)
                {
                    notifications.Add(CreateNotification(removalCancelledAffectedId, groupId, "Remoção Cancelada",
                        $"A votação para remover o evento '{removalCancelledTitle}' foi cancelada.",
                        action, @event?.Id, null));
                }
                break;

            case "shared_event_created":
                var sharedEventTitle = eventName ?? "Evento Compartilhado";
                var sharedEventPoints = points ?? "0";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Novo Evento Compartilhado",
                        $"{performedByName} criou o evento compartilhado '{sharedEventTitle}' com {sharedEventPoints} pontos.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_updated":
                var updatedSharedEventTitle = eventName ?? "Evento Compartilhado";
                var updatedSharedEventOldPoints = GetValue(newValues, "oldPoints");
                var updatedSharedEventNewPoints = GetValue(newValues, "newPoints");

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Evento Compartilhado Atualizado",
                        $"O evento compartilhado '{updatedSharedEventTitle}' teve seus pontos alterados de {updatedSharedEventOldPoints} para {updatedSharedEventNewPoints}.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_deleted":
                var deletedSharedEventTitle = eventName ?? "Evento Compartilhado";
                var participantsAffected = GetValue(newValues, "participantsAffected");

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Evento Compartilhado Deletado",
                        $"O evento compartilhado '{deletedSharedEventTitle}' foi deletado. {participantsAffected} participantes afetados.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_joined":
                var joinedParticipantName = participant ?? "Alguém";
                var joinedEventTitle = eventName ?? "Evento Compartilhado";
                var joinedPoints = points ?? "0";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Novo Participante",
                        $"{joinedParticipantName} entrou no evento compartilhado '{joinedEventTitle}' (+{joinedPoints} pontos).",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_left":
                var leftParticipantName = participant ?? "Alguém";
                var leftEventTitle = eventName ?? "Evento Compartilhado";
                var leftPoints = points ?? "0";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Participante Saiu",
                        $"{leftParticipantName} saiu do evento compartilhado '{leftEventTitle}' (-{leftPoints} pontos).",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_closed":
                var closedEventTitle = eventName ?? "Evento Compartilhado";
                var participantCount = GetValue(newValues, "participantCount");

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Evento Fechado",
                        $"O evento compartilhado '{closedEventTitle}' foi fechado com {participantCount} participantes.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_participant_removal_initiated":
                var removalParticipantTitle = eventName ?? "Evento Compartilhado";
                var removalParticipantName = participant ?? "Alguém";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Remoção de Participante",
                        $"Uma votação foi iniciada para remover {removalParticipantName} do evento '{removalParticipantTitle}'.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_participant_removed_by_vote":
                var removedParticipantTitle = eventName ?? "Evento Compartilhado";
                var removedParticipantName = participant ?? "Alguém";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Participante Removido",
                        $"{removedParticipantName} foi removido do evento '{removedParticipantTitle}' por votação.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "shared_event_participant_removal_cancelled":
                var cancelledParticipantTitle = eventName ?? "Evento Compartilhado";
                var cancelledParticipantName = participant ?? "Alguém";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Remoção Cancelada",
                        $"A votação para remover {cancelledParticipantName} do evento '{cancelledParticipantTitle}' foi cancelada.",
                        action, null, sharedEvent?.Id));
                }
                break;

            case "group_joined":
                var joinedMemberName = GetValue(newValues, "member") ?? "Alguém";
                var joinedGroupName = groupName ?? "Grupo";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Novo Membro",
                        $"{joinedMemberName} entrou no grupo '{joinedGroupName}'.",
                        action, null, null));
                }
                break;

            case "group_left":
                var leftMemberName = GetValue(newValues, "member") ?? "Alguém";
                var leftGroupName = groupName ?? "Grupo";
                var deletedGroup = GetValue(newValues, "deletedGroup");
                var deletedGroupText = deletedGroup == "true" ? " O grupo foi deletado." : "";

                foreach (var memberId in memberIds.Where(id => id != performedByUserId))
                {
                    notifications.Add(CreateNotification(memberId, groupId, "Membro Saiu",
                        $"{leftMemberName} saiu do grupo '{leftGroupName}'.{deletedGroupText}",
                        action, null, null));
                }
                break;

            case "comment_created":
                var commentContent = content ?? "";
                var commentEventId = auditLog.EntityId;
                var commentParentId = parentCommentId;
                var commentAuthor = performedByName ?? "Alguém";

                if (commentParentId != null)
                {
                    // It's a reply - notify the parent comment author
                    var parentCommentAuthorId = GetParentCommentAuthorId(newValues, groupMembers);
                    if (parentCommentAuthorId != Guid.Empty && parentCommentAuthorId != performedByUserId)
                    {
                        notifications.Add(CreateNotification(parentCommentAuthorId, groupId, "Resposta ao seu Comentário",
                            $"{commentAuthor} respondeu ao seu comentário: '{commentContent}'",
                            action, commentEventId, null));
                    }
                }

                // Notify event creator and affected user
                if (@event != null)
                {
                    var eventCreator = @event.CreatedByUserId;
                    var eventAffected = @event.AffectedUserId;

                    if (eventCreator != Guid.Empty && eventCreator != performedByUserId)
                    {
                        notifications.Add(CreateNotification(eventCreator, groupId, "Novo Comentário",
                            $"{commentAuthor} comentou no seu evento: '{commentContent}'",
                            action, @event.Id, null));
                    }

                    if (eventAffected != Guid.Empty && eventAffected != performedByUserId && eventAffected != eventCreator)
                    {
                        notifications.Add(CreateNotification(eventAffected, groupId, "Novo Comentário",
                            $"{commentAuthor} comentou no evento sobre você: '{commentContent}'",
                            action, @event.Id, null));
                    }
                }
                break;
        }

        return notifications;
    }

    private static Notification CreateNotification(Guid userId, Guid groupId, string title, string description, string action, Guid? eventId, Guid? sharedEventId)
    {
        return new Notification
        {
            UserId = userId,
            GroupId = groupId,
            Title = title,
            Description = description,
            Action = action,
            EventId = eventId,
            SharedEventId = sharedEventId
        };
    }

    private static string? GetValue(Dictionary<string, object>? dict, string key)
    {
        if (dict == null) return null;
        return dict.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static Guid GetParentCommentAuthorId(Dictionary<string, object>? dict, IEnumerable<GroupMember> groupMembers)
    {
        var parentId = GetValue(dict, "parentCommentId");
        if (parentId == null) return Guid.Empty;
        
        // This is a simplified approach. In a real scenario, we'd need to look up the parent comment.
        // For now, we'll return empty and let the handler handle this.
        return Guid.Empty;
    }
}
