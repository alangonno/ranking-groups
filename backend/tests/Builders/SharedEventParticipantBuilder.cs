using backend.src.Entities;

namespace backend.tests.Builders;

public class SharedEventParticipantBuilder
{
    private SharedEventParticipant _participant = new();

    public SharedEventParticipantBuilder WithId(Guid id)
    {
        typeof(SharedEventParticipant).GetProperty("Id")?.SetValue(_participant, id);
        return this;
    }

    public SharedEventParticipantBuilder WithSharedEventId(Guid sharedEventId)
    {
        _participant.SharedEventId = sharedEventId;
        return this;
    }

    public SharedEventParticipantBuilder WithSharedEvent(SharedEvent sharedEvent)
    {
        _participant.SharedEvent = sharedEvent;
        _participant.SharedEventId = sharedEvent.Id;
        return this;
    }

    public SharedEventParticipantBuilder WithUserId(Guid userId)
    {
        _participant.UserId = userId;
        return this;
    }

    public SharedEventParticipantBuilder WithUser(User user)
    {
        _participant.User = user;
        _participant.UserId = user.Id;
        return this;
    }

    public SharedEventParticipant Build() => _participant;
}
