using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class EventApprovalRulesTests
{
    [Fact]
    public void ValidateCanVote_WithValidVoter_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var voter = EntityFixtures.CreateUser("Voter");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, voter)
        };
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);
        var approval = new backend.tests.Builders.EventApprovalBuilder()
            .WithEvent(@event)
            .WithUser(voter)
            .Build();

        var act = () => EventApprovalRules.ValidateCanVote(approval, @event, voter, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanVote_AffectedUserVoting_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);
        var approval = new backend.tests.Builders.EventApprovalBuilder()
            .WithEvent(@event)
            .WithUser(affectedUser)
            .Build();

        var act = () => EventApprovalRules.ValidateCanVote(approval, @event, affectedUser, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateCanVote_CreatorVoting_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);
        var approval = new backend.tests.Builders.EventApprovalBuilder()
            .WithEvent(@event)
            .WithUser(creator)
            .Build();

        var act = () => EventApprovalRules.ValidateCanVote(approval, @event, creator, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateCanVote_NonMemberVoting_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var outsider = EntityFixtures.CreateUser("Outsider");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);
        var approval = new backend.tests.Builders.EventApprovalBuilder()
            .WithEvent(@event)
            .WithUser(outsider)
            .Build();

        var act = () => EventApprovalRules.ValidateCanVote(approval, @event, outsider, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateNoDuplicateVote_FirstVote_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingApprovals = new List<EventApproval>();

        var act = () => EventApprovalRules.ValidateNoDuplicateVote(userId, eventId, existingApprovals);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateNoDuplicateVote_SecondVote_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingApprovals = new List<EventApproval>
        {
            new backend.tests.Builders.EventApprovalBuilder()
                .WithUserId(userId)
                .WithEventId(eventId)
                .Build()
        };

        var act = () => EventApprovalRules.ValidateNoDuplicateVote(userId, eventId, existingApprovals);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateEventIsPending_PendingStatus_ShouldNotThrow()
    {
        var act = () => EventApprovalRules.ValidateEventIsPending(EventStatus.Pending);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(EventStatus.Approved)]
    [InlineData(EventStatus.Rejected)]
    [InlineData(EventStatus.Cancelled)]
    public void ValidateEventIsPending_NonPendingStatus_ShouldThrowBusinessRuleException(EventStatus status)
    {
        var act = () => EventApprovalRules.ValidateEventIsPending(status);
        act.Should().Throw<BusinessRuleException>();
    }
}
