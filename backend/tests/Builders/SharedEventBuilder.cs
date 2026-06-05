using backend.src.Entities;

namespace backend.tests.Builders;

public class SharedEventBuilder
{
    private SharedEvent _sharedEvent = new()
    {
        Title = "Test Shared Event",
        Description = "A test shared event",
        Points = 15,
        IsClosed = false
    };

    public SharedEventBuilder WithId(Guid id)
    {
        typeof(SharedEvent).GetProperty("Id")?.SetValue(_sharedEvent, id);
        return this;
    }

    public SharedEventBuilder WithGroupId(Guid groupId)
    {
        _sharedEvent.GroupId = groupId;
        return this;
    }

    public SharedEventBuilder WithGroup(Group group)
    {
        _sharedEvent.Group = group;
        _sharedEvent.GroupId = group.Id;
        return this;
    }

    public SharedEventBuilder WithCreatedByUserId(Guid userId)
    {
        _sharedEvent.CreatedByUserId = userId;
        return this;
    }

    public SharedEventBuilder WithCreatedByUser(User user)
    {
        _sharedEvent.CreatedByUser = user;
        _sharedEvent.CreatedByUserId = user.Id;
        return this;
    }

    public SharedEventBuilder WithTitle(string title)
    {
        _sharedEvent.Title = title;
        return this;
    }

    public SharedEventBuilder WithDescription(string description)
    {
        _sharedEvent.Description = description;
        return this;
    }

    public SharedEventBuilder WithPoints(int points)
    {
        _sharedEvent.Points = points;
        return this;
    }

    public SharedEventBuilder WithIsClosed(bool isClosed)
    {
        _sharedEvent.IsClosed = isClosed;
        return this;
    }

    public SharedEventBuilder WithClosesAt(DateTime? closesAt)
    {
        _sharedEvent.ClosesAt = closesAt;
        return this;
    }

    public SharedEvent Build() => _sharedEvent;
}
