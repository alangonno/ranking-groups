using backend.src.Entities;
using System.Text.Json;

namespace backend.src.Common;

public static class AuditLogBuilder
{
    public static AuditLog EventCreated(Event @event, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = @event.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "event_created",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventUpdated(Event @event, int oldPoints, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            oldPoints = oldPoints,
            newPoints = @event.Points,
            type = @event.Type.ToString(),
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = @event.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "event_updated",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventDeleted(Event @event, Guid performedByUserId, int revertedPoints)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            revertedPoints = revertedPoints,
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = @event.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "event_deleted",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventApproved(Event @event, Guid performedByUserId, int appliedPoints)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            appliedPoints = appliedPoints,
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "event_approved",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventRejectedDeleted(Event @event, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            rejectedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "event_rejected_deleted",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventCreated(SharedEvent sharedEvent, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = sharedEvent.Points,
            performedBy = sharedEvent.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "shared_event_created",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventUpdated(SharedEvent sharedEvent, int oldPoints, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            oldPoints = oldPoints,
            newPoints = sharedEvent.Points,
            performedBy = sharedEvent.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "shared_event_updated",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventDeleted(SharedEvent sharedEvent, int participantsAffected, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = sharedEvent.Points,
            participantsAffected = participantsAffected,
            performedBy = sharedEvent.CreatedByUser?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "shared_event_deleted",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventJoined(SharedEvent sharedEvent, Guid userId, string userName, int points)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = points,
            participant = userName
        });

        return new AuditLog
        {
            Action = "shared_event_joined",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventLeft(SharedEvent sharedEvent, Guid userId, string userName, int points)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = points,
            participant = userName
        });

        return new AuditLog
        {
            Action = "shared_event_left",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventClosed(SharedEvent sharedEvent, int participantCount, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = sharedEvent.Points,
            participantCount = participantCount,
            performedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "shared_event_closed",
            EntityName = "SharedEvent",
            EntityId = sharedEvent.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventRemovalInitiated(Event @event, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "event_removal_initiated",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventRemovedByVote(Event @event, Guid performedByUserId, int revertedPoints)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            revertedPoints = revertedPoints,
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "event_removed_by_vote",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog EventRemovalCancelled(Event @event, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = @event.Title,
            points = @event.Points,
            type = @event.Type.ToString(),
            affectedUser = @event.AffectedUser?.Name ?? string.Empty,
            performedBy = performedByUserId
        });

        return new AuditLog
        {
            Action = "event_removal_cancelled",
            EntityName = "Event",
            EntityId = @event.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventParticipantRemovedByVote(SharedEvent sharedEvent, Guid userId, string userName)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = sharedEvent.Points,
            participant = userName
        });

        return new AuditLog
        {
            Action = "shared_event_participant_removed_by_vote",
            EntityName = "SharedEventParticipant",
            EntityId = sharedEvent.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog SharedEventParticipantRemovalCancelled(SharedEvent sharedEvent, Guid userId, string userName)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            eventName = sharedEvent.Title,
            points = sharedEvent.Points,
            participant = userName
        });

        return new AuditLog
        {
            Action = "shared_event_participant_removal_cancelled",
            EntityName = "SharedEventParticipant",
            EntityId = sharedEvent.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog GroupJoined(Group group, Guid userId, string userName)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            groupName = group.Name,
            member = userName
        });

        return new AuditLog
        {
            Action = "group_joined",
            EntityName = "Group",
            EntityId = group.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog GroupLeft(Group group, Guid userId, string userName, bool deletedGroup)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            groupName = group.Name,
            member = userName,
            deletedGroup = deletedGroup
        });

        return new AuditLog
        {
            Action = "group_left",
            EntityName = "Group",
            EntityId = group.Id,
            PerformedByUserId = userId,
            NewValues = newValues
        };
    }

    public static AuditLog CommentCreated(Comment comment, Guid performedByUserId)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            content = comment.Content,
            eventId = comment.EventId,
            sharedEventId = comment.SharedEventId,
            parentCommentId = comment.ParentCommentId,
            performedBy = comment.User?.Name ?? string.Empty
        });

        return new AuditLog
        {
            Action = "comment_created",
            EntityName = "Comment",
            EntityId = comment.Id,
            PerformedByUserId = performedByUserId,
            NewValues = newValues
        };
    }
}
