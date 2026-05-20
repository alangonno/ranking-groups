using backend.src.Common.Enums;
using backend.src.Entities;

namespace backend.tests.Builders;

public class EventApprovalBuilder
{
    private EventApproval _approval = new()
    {
        VoteType = EventVoteType.Approve
    };

    public EventApprovalBuilder WithId(Guid id)
    {
        typeof(EventApproval).GetProperty("Id")?.SetValue(_approval, id);
        return this;
    }

    public EventApprovalBuilder WithEventId(Guid eventId)
    {
        _approval.EventId = eventId;
        return this;
    }

    public EventApprovalBuilder WithEvent(Event @event)
    {
        _approval.Event = @event;
        _approval.EventId = @event.Id;
        return this;
    }

    public EventApprovalBuilder WithUserId(Guid userId)
    {
        _approval.UserId = userId;
        return this;
    }

    public EventApprovalBuilder WithUser(User user)
    {
        _approval.User = user;
        _approval.UserId = user.Id;
        return this;
    }

    public EventApprovalBuilder WithVoteType(EventVoteType voteType)
    {
        _approval.VoteType = voteType;
        return this;
    }

    public EventApproval Build() => _approval;
}
