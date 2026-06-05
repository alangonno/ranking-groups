using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Handlers.Jobs;
using backend.src.Repositories;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.tests.Handlers.Jobs;

public class ResolveExpiredVotesJobHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ApprovalExpired_WithQuorum_ShouldApprove()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var affected = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");

        context.Groups.Add(group);
        context.Users.AddRange(creator, affected, voter1, voter2);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affected),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2)
        );

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affected)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Pending)
            .WithPoints(10)
            .Build();
        typeof(Event).GetProperty("ApprovalDeadline")?.SetValue(@event, DateTime.UtcNow.AddHours(-1));
        context.Events.Add(@event);

        context.EventApprovals.AddRange(
            new EventApproval { EventId = @event.Id, UserId = creator.Id, VoteType = EventVoteType.Approve },
            new EventApproval { EventId = @event.Id, UserId = voter1.Id, VoteType = EventVoteType.Approve }
        );
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.ApprovedEventsCount.Should().Be(1);
        var updated = await context.Events.FirstAsync();
        updated.Status.Should().Be(EventStatus.Approved);
    }

    [Fact]
    public async Task ProcessAsync_ApprovalExpired_WithoutQuorum_ShouldReject()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var affected = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");

        context.Groups.Add(group);
        context.Users.AddRange(creator, affected, voter1, voter2);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affected),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2)
        );

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affected)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Pending)
            .WithPoints(10)
            .Build();
        typeof(Event).GetProperty("ApprovalDeadline")?.SetValue(@event, DateTime.UtcNow.AddHours(-1));
        context.Events.Add(@event);

        context.EventApprovals.Add(new EventApproval { EventId = @event.Id, UserId = creator.Id, VoteType = EventVoteType.Approve });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.RejectedEventsCount.Should().Be(1);
        var count = await context.Events.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_EventRemovalExpired_WithRemoveQuorum_ShouldRemove()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var affected = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");
        var voter3 = EntityFixtures.CreateUser("Voter3");

        context.Groups.Add(group);
        context.Users.AddRange(creator, affected, voter1, voter2, voter3);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affected),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2),
            EntityFixtures.CreateGroupMember(group, voter3)
        );

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affected)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();
        @event.IsPendingRemoval = true;
        @event.RemovalVoteDeadline = DateTime.UtcNow.AddHours(-1);
        context.Events.Add(@event);

        context.EventApprovals.AddRange(
            new EventApproval { EventId = @event.Id, UserId = voter1.Id, VoteType = EventVoteType.Remove },
            new EventApproval { EventId = @event.Id, UserId = voter2.Id, VoteType = EventVoteType.Remove },
            new EventApproval { EventId = @event.Id, UserId = voter3.Id, VoteType = EventVoteType.Remove }
        );
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.RemovedEventsCount.Should().Be(1);
        var count = await context.Events.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_EventRemovalExpired_WithoutQuorum_ShouldKeep()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var affected = EntityFixtures.CreateUser("Affected");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");

        context.Groups.Add(group);
        context.Users.AddRange(creator, affected, voter1, voter2);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affected),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2)
        );

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affected)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();
        @event.IsPendingRemoval = true;
        @event.RemovalVoteDeadline = DateTime.UtcNow.AddHours(-1);
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.KeptEventsCount.Should().Be(1);
        var updated = await context.Events.FirstAsync();
        updated.IsPendingRemoval.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_ParticipantRemovalExpired_WithRemoveQuorum_ShouldRemove()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var participant = EntityFixtures.CreateUser("Participant");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");
        var voter3 = EntityFixtures.CreateUser("Voter3");

        context.Groups.Add(group);
        context.Users.AddRange(creator, participant, voter1, voter2, voter3);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, participant),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2),
            EntityFixtures.CreateGroupMember(group, voter3)
        );

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithPoints(20)
            .Build();
        context.SharedEvents.Add(sharedEvent);

        var participantEntity = new SharedEventParticipant
        {
            SharedEventId = sharedEvent.Id,
            UserId = participant.Id,
            IsPendingRemoval = true,
            RemovalVoteDeadline = DateTime.UtcNow.AddHours(-1)
        };
        context.SharedEventParticipants.Add(participantEntity);

        context.SharedEventParticipantRemovalVotes.AddRange(
            new SharedEventParticipantRemovalVote { SharedEventId = sharedEvent.Id, ParticipantId = participantEntity.Id, UserId = voter1.Id, VoteType = EventVoteType.Remove },
            new SharedEventParticipantRemovalVote { SharedEventId = sharedEvent.Id, ParticipantId = participantEntity.Id, UserId = voter2.Id, VoteType = EventVoteType.Remove },
            new SharedEventParticipantRemovalVote { SharedEventId = sharedEvent.Id, ParticipantId = participantEntity.Id, UserId = voter3.Id, VoteType = EventVoteType.Remove }
        );
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.RemovedParticipantsCount.Should().Be(1);
        var count = await context.SharedEventParticipants.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_ParticipantRemovalExpired_WithoutQuorum_ShouldKeep()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var participant = EntityFixtures.CreateUser("Participant");
        var voter1 = EntityFixtures.CreateUser("Voter1");
        var voter2 = EntityFixtures.CreateUser("Voter2");

        context.Groups.Add(group);
        context.Users.AddRange(creator, participant, voter1, voter2);
        context.GroupMembers.AddRange(
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, participant),
            EntityFixtures.CreateGroupMember(group, voter1),
            EntityFixtures.CreateGroupMember(group, voter2)
        );

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithPoints(20)
            .Build();
        context.SharedEvents.Add(sharedEvent);

        var participantEntity = new SharedEventParticipant
        {
            SharedEventId = sharedEvent.Id,
            UserId = participant.Id,
            IsPendingRemoval = true,
            RemovalVoteDeadline = DateTime.UtcNow.AddHours(-1)
        };
        context.SharedEventParticipants.Add(participantEntity);
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var result = await handler.ProcessAsync(DateTime.UtcNow, CancellationToken.None);

        result.KeptParticipantsCount.Should().Be(1);
        var updated = await context.SharedEventParticipants.FirstAsync();
        updated.IsPendingRemoval.Should().BeFalse();
    }

    private static ResolveExpiredVotesJobHandler CreateHandler(AppDbContext context)
    {
        return new ResolveExpiredVotesJobHandler(
            new EventRepository(context),
            new EventApprovalRepository(context),
            new GroupMemberRepository(context),
            new SharedEventParticipantRepository(context),
            new SharedEventParticipantRemovalVoteRepository(context),
            new AuditLogRepository(context),
            new NotificationRepository(context),
            context
        );
    }
}
