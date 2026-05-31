using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class SharedEventParticipantRemovalRulesTests
{
    [Fact]
    public void ValidateCanInitiateRemoval_WithValidParticipant_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var initiator = EntityFixtures.CreateUser("Initiator");
        var participant = EntityFixtures.CreateParticipant(
            EntityFixtures.CreateSharedEvent(group, EntityFixtures.CreateUser("Creator")),
            EntityFixtures.CreateUser("Participant"));
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, initiator)
        };

        var act = () => SharedEventParticipantRemovalRules.ValidateCanInitiateRemoval(participant, initiator.Id, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanInitiateRemoval_WithPendingRemoval_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var initiator = EntityFixtures.CreateUser("Initiator");
        var participant = EntityFixtures.CreateParticipant(
            EntityFixtures.CreateSharedEvent(group, EntityFixtures.CreateUser("Creator")),
            EntityFixtures.CreateUser("Participant"));
        typeof(SharedEventParticipant).GetProperty("IsPendingRemoval")?.SetValue(participant, true);
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, initiator)
        };

        var act = () => SharedEventParticipantRemovalRules.ValidateCanInitiateRemoval(participant, initiator.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("participant_already_pending_removal");
    }

    [Fact]
    public void ValidateCanVoteRemoval_WithValidVoter_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var voter = EntityFixtures.CreateUser("Voter");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, voter),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var existingVotes = new List<SharedEventParticipantRemovalVote>();

        var act = () => SharedEventParticipantRemovalRules.ValidateCanVoteRemoval(voter.Id, affectedUser.Id, existingVotes, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanVoteRemoval_AffectedUserVoting_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var existingVotes = new List<SharedEventParticipantRemovalVote>();

        var act = () => SharedEventParticipantRemovalRules.ValidateCanVoteRemoval(affectedUser.Id, affectedUser.Id, existingVotes, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("affected_user_cannot_vote_removal");
    }

    [Fact]
    public void ValidateCanVoteRemoval_DuplicateVote_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var voter = EntityFixtures.CreateUser("Voter");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, voter),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };
        var sharedEvent = EntityFixtures.CreateSharedEvent(group, EntityFixtures.CreateUser("Creator"));
        var participant = EntityFixtures.CreateParticipant(sharedEvent, affectedUser);
        var existingVotes = new List<SharedEventParticipantRemovalVote>
        {
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter.Id, VoteType = EventVoteType.Remove }
        };

        var act = () => SharedEventParticipantRemovalRules.ValidateCanVoteRemoval(voter.Id, affectedUser.Id, existingVotes, groupMembers);
        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("duplicate_vote_not_allowed");
    }

    [Fact]
    public void ValidateVoteDeadline_BeforeDeadline_ShouldNotThrow()
    {
        var deadline = DateTime.UtcNow.AddHours(1);

        var act = () => SharedEventParticipantRemovalRules.ValidateVoteDeadline(deadline);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateVoteDeadline_AfterDeadline_ShouldThrowBusinessRuleException()
    {
        var deadline = DateTime.UtcNow.AddHours(-1);

        var act = () => SharedEventParticipantRemovalRules.ValidateVoteDeadline(deadline);

        act.Should().Throw<BusinessRuleException>().Which.Rule.Should().Be("removal_vote_deadline_expired");
    }

    [Fact]
    public void CalculateQuorum_3Members_ShouldReturn1()
    {
        var result = SharedEventParticipantRemovalRules.CalculateQuorum(3);
        result.Should().Be(1);
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
        var sharedEvent = EntityFixtures.CreateSharedEvent(group, creator);
        var participant = EntityFixtures.CreateParticipant(sharedEvent, affectedUser);
        var existingVotes = new List<SharedEventParticipantRemovalVote>
        {
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter1.Id, VoteType = EventVoteType.Remove },
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter2.Id, VoteType = EventVoteType.Remove },
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter3.Id, VoteType = EventVoteType.Remove },
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter4.Id, VoteType = EventVoteType.Remove }
        };

        // 6 membros → quorum=2. Remove=4 >= 2 E 4 > 2 (creator+affectedUser não-votantes) → Remove vence
        var result = SharedEventParticipantRemovalRules.ResolveExpiredRemovalVote(groupMembers, existingVotes);

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
        var sharedEvent = EntityFixtures.CreateSharedEvent(group, creator);
        var participant = EntityFixtures.CreateParticipant(sharedEvent, affectedUser);
        var existingVotes = new List<SharedEventParticipantRemovalVote>
        {
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter1.Id, VoteType = EventVoteType.Remove }
        };

        // Remove=1, Keep=0+2=2 → Keep vence
        var result = SharedEventParticipantRemovalRules.ResolveExpiredRemovalVote(groupMembers, existingVotes);

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
        var sharedEvent = EntityFixtures.CreateSharedEvent(group, creator);
        var participant = EntityFixtures.CreateParticipant(sharedEvent, affectedUser);
        var existingVotes = new List<SharedEventParticipantRemovalVote>
        {
            new() { SharedEventId = sharedEvent.Id, ParticipantId = participant.Id, UserId = voter1.Id, VoteType = EventVoteType.Remove }
        };

        // Remove=1, Keep=0+1=1 → empate → Keep vence
        var result = SharedEventParticipantRemovalRules.ResolveExpiredRemovalVote(groupMembers, existingVotes);

        result.Should().Be(RemovalResolution.Keep);
    }
}
