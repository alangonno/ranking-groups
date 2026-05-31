using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class EventRemovalRulesTests
{
    [Fact]
    public void ValidateCanInitiateRemoval_WithValidEvent_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var initiator = EntityFixtures.CreateUser("Initiator");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, initiator)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);

        var act = () => EventRemovalRules.ValidateCanInitiateRemoval(@event, initiator.Id, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanInitiateRemoval_WithPendingEvent_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var initiator = EntityFixtures.CreateUser("Initiator");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, initiator)
        };
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);

        var act = () => EventRemovalRules.ValidateCanInitiateRemoval(@event, initiator.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("event_not_approved");
    }

    [Fact]
    public void ValidateCanInitiateRemoval_WithAlreadyPendingRemoval_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var initiator = EntityFixtures.CreateUser("Initiator");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, initiator)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        typeof(Event).GetProperty("IsPendingRemoval")?.SetValue(@event, true);

        var act = () => EventRemovalRules.ValidateCanInitiateRemoval(@event, initiator.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("event_already_pending_removal");
    }

    [Fact]
    public void ValidateCanInitiateRemoval_WithNonMember_ShouldThrowBusinessRuleException()
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
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);

        var act = () => EventRemovalRules.ValidateCanInitiateRemoval(@event, outsider.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("non_member_cannot_initiate_removal");
    }

    [Fact]
    public void ValidateCanVoteRemoval_WithValidVoter_ShouldNotThrow()
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
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var existingApprovals = new List<EventApproval>
        {
            EntityFixtures.CreateApproval(@event, creator, EventVoteType.Keep)
        };

        var act = () => EventRemovalRules.ValidateCanVoteRemoval(voter.Id, @event, existingApprovals, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanVoteRemoval_AffectedUserVoting_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var existingApprovals = new List<EventApproval>();

        var act = () => EventRemovalRules.ValidateCanVoteRemoval(affectedUser.Id, @event, existingApprovals, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("affected_user_cannot_vote_removal");
    }

    [Fact]
    public void ValidateCanVoteRemoval_DuplicateVote_ShouldThrowBusinessRuleException()
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
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var existingApprovals = new List<EventApproval>
        {
            EntityFixtures.CreateApproval(@event, voter, EventVoteType.Remove)
        };

        var act = () => EventRemovalRules.ValidateCanVoteRemoval(voter.Id, @event, existingApprovals, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("duplicate_vote_not_allowed");
    }

    [Fact]
    public void ValidateEventIsPendingRemoval_True_ShouldNotThrow()
    {
        var act = () => EventRemovalRules.ValidateEventIsPendingRemoval(true);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateEventIsPendingRemoval_False_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRemovalRules.ValidateEventIsPendingRemoval(false);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("event_not_pending_removal");
    }

    [Fact]
    public void CalculateQuorum_3Members_ShouldReturn1()
    {
        var result = EventRemovalRules.CalculateQuorum(3);
        result.Should().Be(1);
    }

    [Fact]
    public void CalculateQuorum_4Members_ShouldReturn2()
    {
        var result = EventRemovalRules.CalculateQuorum(4);
        result.Should().Be(2);
    }

    [Fact]
    public void CalculateQuorum_5Members_ShouldReturn2()
    {
        var result = EventRemovalRules.CalculateQuorum(5);
        result.Should().Be(2);
    }

    [Fact]
    public void CalculateQuorum_6Members_ShouldReturn2()
    {
        var result = EventRemovalRules.CalculateQuorum(6);
        result.Should().Be(2);
    }

    [Fact]
    public void ValidateRemoveQuorum_RemoveWins_ShouldNotThrow()
    {
        var act = () => EventRemovalRules.ValidateRemoveQuorum(
            removeCount: 3, keepCount: 1, totalMembers: 6);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateRemoveQuorum_KeepWins_ShouldNotThrow()
    {
        var act = () => EventRemovalRules.ValidateRemoveQuorum(
            removeCount: 1, keepCount: 3, totalMembers: 6);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateRemoveQuorum_NeitherSideMet_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRemovalRules.ValidateRemoveQuorum(
            removeCount: 1, keepCount: 1, totalMembers: 6);

        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("removal_quorum_not_reached");
    }

    [Fact]
    public void ValidateRemoveQuorum_RemoveBelowQuorum_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRemovalRules.ValidateRemoveQuorum(
            removeCount: 1, keepCount: 0, totalMembers: 6);

        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("removal_quorum_not_reached");
    }

    [Fact]
    public void ValidateRemoveQuorum_KeepBelowQuorum_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRemovalRules.ValidateRemoveQuorum(
            removeCount: 0, keepCount: 1, totalMembers: 6);

        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("removal_quorum_not_reached");
    }

    [Fact]
    public void IsBypassRemoval_AffectedUserRemovingPositiveEvent_ShouldReturnTrue()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);

        var result = EventRemovalRules.IsBypassRemoval(@event, affectedUser.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsBypassRemoval_AffectedUserRemovingNegativeEvent_ShouldReturnFalse()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var @event = EntityFixtures.CreateNegativeEvent(group, creator, affectedUser);
        typeof(Event).GetProperty("Status")?.SetValue(@event, EventStatus.Approved);

        var result = EventRemovalRules.IsBypassRemoval(@event, affectedUser.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsBypassRemoval_OtherUserRemovingPositiveEvent_ShouldReturnFalse()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var otherUser = EntityFixtures.CreateUser("Other");
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);

        var result = EventRemovalRules.IsBypassRemoval(@event, otherUser.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateVoteDeadline_BeforeDeadline_ShouldNotThrow()
    {
        var deadline = DateTime.UtcNow.AddHours(1);

        var act = () => EventRemovalRules.ValidateVoteDeadline(deadline);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateVoteDeadline_AfterDeadline_ShouldThrowBusinessRuleException()
    {
        var deadline = DateTime.UtcNow.AddHours(-1);

        var act = () => EventRemovalRules.ValidateVoteDeadline(deadline);

        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("removal_vote_deadline_expired");
    }

    [Fact]
    public void ResolveExpiredRemovalVote_RemoveReachesQuorum_ShouldReturnRemove()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");
        var voter3 = EntityFixtures.CreateUser("Voter3");
        var voter4 = EntityFixtures.CreateUser("Voter4");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2),
            EntityFixtures.CreateGroupMember(group, voter3),
            EntityFixtures.CreateGroupMember(group, voter4)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var approvals = new List<EventApproval>
        {
            EntityFixtures.CreateApproval(@event, voter1, EventVoteType.Remove),
            EntityFixtures.CreateApproval(@event, voter2, EventVoteType.Remove),
            EntityFixtures.CreateApproval(@event, voter3, EventVoteType.Remove),
            EntityFixtures.CreateApproval(@event, voter4, EventVoteType.Remove)
        };

        // 6 membros → quorum=2. Remove=4 >= 2 E 4 > 2 (creator+affectedUser não-votantes) → Remove vence
        var result = EventRemovalRules.ResolveExpiredRemovalVote(@event, groupMembers, approvals);

        result.Should().Be(RemovalResolution.Remove);
    }

    [Fact]
    public void ResolveExpiredRemovalVote_NonVotersCountAsKeep_ShouldReturnKeep()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var nonVoter1 = EntityFixtures.CreateUser("NonVoter1");
        var nonVoter2 = EntityFixtures.CreateUser("NonVoter2");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, nonVoter1),
            EntityFixtures.CreateGroupMember(group, nonVoter2)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var approvals = new List<EventApproval>
        {
            EntityFixtures.CreateApproval(@event, voter1, EventVoteType.Remove)
        };

        var result = EventRemovalRules.ResolveExpiredRemovalVote(@event, groupMembers, approvals);

        // Remove=1, Keep=0+2=2 → Keep vence
        result.Should().Be(RemovalResolution.Keep);
    }

    [Fact]
    public void ResolveExpiredRemovalVote_Tie_ShouldReturnKeep()
    {
        var group = EntityFixtures.CreateGroup();
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var nonVoter = EntityFixtures.CreateUser("NonVoter");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, nonVoter)
        };
        var @event = EntityFixtures.CreatePositiveEvent(group, creator, affectedUser);
        var approvals = new List<EventApproval>
        {
            EntityFixtures.CreateApproval(@event, voter1, EventVoteType.Remove)
        };

        var result = EventRemovalRules.ResolveExpiredRemovalVote(@event, groupMembers, approvals);

        // Remove=1, Keep=0+1=1 → empate → Keep vence
        result.Should().Be(RemovalResolution.Keep);
    }
}
