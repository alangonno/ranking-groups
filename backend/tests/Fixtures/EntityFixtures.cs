using backend.src.Common.Enums;
using backend.src.Entities;
using backend.tests.Builders;

namespace backend.tests.Fixtures;

public static class EntityFixtures
{
    public static User CreateUser(string? name = null)
    {
        return new UserBuilder()
            .WithName(name ?? "Test User")
            .Build();
    }

    public static Group CreateGroup(User? createdBy = null)
    {
        var creator = createdBy ?? CreateUser("Group Owner");
        return new GroupBuilder()
            .WithCreatedByUser(creator)
            .Build();
    }

    public static GroupMember CreateGroupMember(Group group, User user, GroupRole role = GroupRole.Member)
    {
        return new GroupMemberBuilder()
            .WithGroup(group)
            .WithUser(user)
            .WithRole(role)
            .Build();
    }

    public static Event CreatePositiveEvent(Group group, User creator, User affectedUser)
    {
        return new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();
    }

    public static Event CreateNegativeEvent(Group group, User creator, User affectedUser)
    {
        return new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Pending)
            .WithPoints(15)
            .Build();
    }

    public static SharedEvent CreateSharedEvent(Group group, User creator, int points = 20)
    {
        return new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithPoints(points)
            .Build();
    }

    public static EventApproval CreateApproval(Event @event, User voter, EventVoteType voteType = EventVoteType.Approve)
    {
        return new EventApprovalBuilder()
            .WithEvent(@event)
            .WithUser(voter)
            .WithVoteType(voteType)
            .Build();
    }

    public static SharedEventParticipant CreateParticipant(SharedEvent sharedEvent, User user)
    {
        return new SharedEventParticipantBuilder()
            .WithSharedEvent(sharedEvent)
            .WithUser(user)
            .Build();
    }
}
