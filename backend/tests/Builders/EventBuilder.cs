using backend.src.Common.Enums;
using backend.src.Entities;

namespace backend.tests.Builders;

public class EventBuilder
{
    private Event _event = new()
    {
        Title = "Test Event",
        Description = "A test event",
        Points = 10,
        Type = EventType.Positive,
        Status = EventStatus.Approved
    };

    public EventBuilder WithId(Guid id)
    {
        typeof(Event).GetProperty("Id")?.SetValue(_event, id);
        return this;
    }

    public EventBuilder WithGroupId(Guid groupId)
    {
        _event.GroupId = groupId;
        return this;
    }

    public EventBuilder WithGroup(Group group)
    {
        _event.Group = group;
        _event.GroupId = group.Id;
        return this;
    }

    public EventBuilder WithCreatedByUserId(Guid userId)
    {
        _event.CreatedByUserId = userId;
        return this;
    }

    public EventBuilder WithCreatedByUser(User user)
    {
        _event.CreatedByUser = user;
        _event.CreatedByUserId = user.Id;
        return this;
    }

    public EventBuilder WithAffectedUserId(Guid userId)
    {
        _event.AffectedUserId = userId;
        return this;
    }

    public EventBuilder WithAffectedUser(User user)
    {
        _event.AffectedUser = user;
        _event.AffectedUserId = user.Id;
        return this;
    }

    public EventBuilder WithTitle(string title)
    {
        _event.Title = title;
        return this;
    }

    public EventBuilder WithDescription(string description)
    {
        _event.Description = description;
        return this;
    }

    public EventBuilder WithPoints(int points)
    {
        _event.Points = points;
        return this;
    }

    public EventBuilder WithType(EventType type)
    {
        _event.Type = type;
        return this;
    }

    public EventBuilder WithStatus(EventStatus status)
    {
        _event.Status = status;
        return this;
    }

    public EventBuilder WithApprovedAt(DateTime? approvedAt)
    {
        _event.ApprovedAt = approvedAt;
        return this;
    }

    public EventBuilder WithRejectedAt(DateTime? rejectedAt)
    {
        _event.RejectedAt = rejectedAt;
        return this;
    }

    public EventBuilder WithCancelledAt(DateTime? cancelledAt)
    {
        _event.CancelledAt = cancelledAt;
        return this;
    }

    public Event Build() => _event;
}
