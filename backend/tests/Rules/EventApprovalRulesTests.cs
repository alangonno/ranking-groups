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
    public void ValidateCanVote_CreatorVoting_ShouldNotThrow()
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
        act.Should().NotThrow();
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

    [Fact]
    public void ValidateApprovalQuorum_3Members_1Approval_ShouldNotThrow()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 1, totalMembers: 3);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateApprovalQuorum_3Members_0Approvals_ShouldThrowBusinessRuleException()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 0, totalMembers: 3);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateApprovalQuorum_5Members_2Approvals_ShouldNotThrow()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 2, totalMembers: 5);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateApprovalQuorum_5Members_1Approval_ShouldThrowBusinessRuleException()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 1, totalMembers: 5);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateApprovalQuorum_2Members_1Approval_ShouldNotThrow()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 1, totalMembers: 2);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateApprovalQuorum_4Members_2Approvals_ShouldNotThrow()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 2, totalMembers: 4);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateApprovalQuorum_4Members_1Approval_ShouldThrowBusinessRuleException()
    {
        var act = () => EventApprovalRules.ValidateApprovalQuorum(approvalCount: 1, totalMembers: 4);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ResolveExpiredApprovalVote_3Members_2Approves_ShouldApprove()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 3, approveCount: 2, rejectCount: 0);
        result.Should().Be(ApprovalResolution.Approve);
    }

    [Fact]
    public void ResolveExpiredApprovalVote_3Members_2Rejects_ShouldReject()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 3, approveCount: 0, rejectCount: 2);
        result.Should().Be(ApprovalResolution.Reject);
    }

    [Fact]
    public void ResolveExpiredApprovalVote_5Members_1Approve_0Reject_ShouldReject()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 5, approveCount: 1, rejectCount: 0);
        result.Should().Be(ApprovalResolution.Reject);
    }

    [Fact]
    public void ResolveExpiredApprovalVote_5Members_1Approve_1Reject_ShouldReject()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 5, approveCount: 1, rejectCount: 1);
        result.Should().Be(ApprovalResolution.Reject);
    }

    [Fact]
    public void ResolveExpiredApprovalVote_5Members_2Approves_1Reject_ShouldApprove()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 5, approveCount: 2, rejectCount: 1);
        result.Should().Be(ApprovalResolution.Approve);
    }

    [Fact]
    public void ResolveExpiredApprovalVote_5Members_2Approves_2Rejects_ShouldReject()
    {
        var result = EventApprovalRules.ResolveExpiredApprovalVote(totalMembers: 5, approveCount: 2, rejectCount: 2);
        result.Should().Be(ApprovalResolution.Reject);
    }
}
