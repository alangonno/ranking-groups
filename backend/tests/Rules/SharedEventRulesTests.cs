using backend.src.Common.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class SharedEventRulesTests
{
    [Fact]
    public void ValidatePositiveOnly_PositiveType_ShouldNotThrow()
    {
        var act = () => SharedEventRules.ValidatePositiveOnly(EventType.Positive);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePositiveOnly_NegativeType_ShouldThrowBusinessRuleException()
    {
        var act = () => SharedEventRules.ValidatePositiveOnly(EventType.Negative);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidatePoints_WithPositivePoints_ShouldNotThrow()
    {
        var act = () => SharedEventRules.ValidatePoints(10);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePoints_WithZeroPoints_ShouldThrowBusinessRuleException()
    {
        var act = () => SharedEventRules.ValidatePoints(0);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidatePoints_WithNegativePoints_ShouldThrowBusinessRuleException()
    {
        var act = () => SharedEventRules.ValidatePoints(-5);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateGroupMembership_MemberParticipating_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var member = EntityFixtures.CreateUser("Member");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, member)
        };

        var act = () => SharedEventRules.ValidateGroupMembership(member.Id, group.Id, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateGroupMembership_NonMemberParticipating_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var member = EntityFixtures.CreateUser("Member");
        var outsider = EntityFixtures.CreateUser("Outsider");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, member)
        };

        var act = () => SharedEventRules.ValidateGroupMembership(outsider.Id, group.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateNotClosed_OpenEvent_ShouldNotThrow()
    {
        var act = () => SharedEventRules.ValidateNotClosed(false);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateNotClosed_ClosedEvent_ShouldThrowBusinessRuleException()
    {
        var act = () => SharedEventRules.ValidateNotClosed(true);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateNoDuplicateParticipation_FirstParticipation_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var existingParticipants = new List<SharedEventParticipant>();

        var act = () => SharedEventRules.ValidateNoDuplicateParticipation(userId, sharedEventId, existingParticipants);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateNoDuplicateParticipation_SecondParticipation_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var existingParticipants = new List<SharedEventParticipant>
        {
            new backend.tests.Builders.SharedEventParticipantBuilder()
                .WithUserId(userId)
                .WithSharedEventId(sharedEventId)
                .Build()
        };

        var act = () => SharedEventRules.ValidateNoDuplicateParticipation(userId, sharedEventId, existingParticipants);
        act.Should().Throw<BusinessRuleException>();
    }
}
