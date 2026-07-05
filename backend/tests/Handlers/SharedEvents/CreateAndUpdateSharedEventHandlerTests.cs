using backend.src.Data;
using backend.src.Handlers.SharedEvents;
using backend.src.Repositories;
using backend.src.Services;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.SharedEvents;

public class CreateAndUpdateSharedEventHandlerTests
{
    [Fact]
    public async Task CreateSharedEvent_WithParticipantUserIds_ShouldCreateParticipantsAndApplyScoreToEachSelectedMember()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var firstParticipant = EntityFixtures.CreateUser("First Participant");
        var secondParticipant = EntityFixtures.CreateUser("Second Participant");

        var memberships = new[]
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, firstParticipant),
            EntityFixtures.CreateGroupMember(group, secondParticipant)
        };

        context.Groups.Add(group);
        context.Users.AddRange(creator, firstParticipant, secondParticipant);
        context.GroupMembers.AddRange(memberships);
        await context.SaveChangesAsync();

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(creator.Id);

        var storageService = Substitute.For<ISupabaseStorageService>();
        storageService.GetPublicUrlFromPath(Arg.Any<string?>()).Returns(string.Empty);

        var handler = new CreateSharedEventHandler(
            new SharedEventRepository(context),
            new SharedEventParticipantRepository(context),
            new GroupRepository(context),
            new GroupMemberRepository(context),
            currentUserService,
            new AuditLogRepository(context),
            new NotificationRepository(context),
            storageService,
            context);

        var request = new CreateSharedEventRequest
        {
            GroupId = group.Id,
            Title = "Churrasco",
            Description = "Evento com participantes já adicionados",
            Points = 20,
            ParticipantUserIds = new List<Guid> { firstParticipant.Id, secondParticipant.Id }
        };

        var response = await handler.HandleAsync(request, CancellationToken.None);

        response.Title.Should().Be("Churrasco");

        var sharedEvent = await context.SharedEvents
            .Include(se => se.Participants)
            .SingleAsync();

        sharedEvent.Participants.Select(p => p.UserId)
            .Should()
            .BeEquivalentTo(new[] { firstParticipant.Id, secondParticipant.Id });

        var updatedMemberships = await context.GroupMembers
            .Where(gm => gm.GroupId == group.Id)
            .ToListAsync();

        updatedMemberships.Single(gm => gm.UserId == creator.Id).CurrentScore.Should().Be(0);
        updatedMemberships.Single(gm => gm.UserId == firstParticipant.Id).CurrentScore.Should().Be(20);
        updatedMemberships.Single(gm => gm.UserId == secondParticipant.Id).CurrentScore.Should().Be(20);
    }

    [Fact]
    public async Task UpdateSharedEvent_WithParticipantDiffAndPointsChange_ShouldRecalculateSelectedMembersScores()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var removedParticipant = EntityFixtures.CreateUser("Removed Participant");
        var keptParticipant = EntityFixtures.CreateUser("Kept Participant");
        var addedParticipant = EntityFixtures.CreateUser("Added Participant");

        var creatorMembership = EntityFixtures.CreateGroupMember(group, creator);
        var removedMembership = new GroupMemberBuilder()
            .WithGroup(group)
            .WithUser(removedParticipant)
            .WithCurrentScore(10)
            .Build();
        var keptMembership = new GroupMemberBuilder()
            .WithGroup(group)
            .WithUser(keptParticipant)
            .WithCurrentScore(10)
            .Build();
        var addedMembership = EntityFixtures.CreateGroupMember(group, addedParticipant);

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithTitle("Evento original")
            .WithDescription("Descricao original")
            .WithPoints(10)
            .Build();

        var originalParticipants = new[]
        {
            new SharedEventParticipantBuilder().WithSharedEvent(sharedEvent).WithUser(removedParticipant).Build(),
            new SharedEventParticipantBuilder().WithSharedEvent(sharedEvent).WithUser(keptParticipant).Build()
        };

        context.Groups.Add(group);
        context.Users.AddRange(creator, removedParticipant, keptParticipant, addedParticipant);
        context.GroupMembers.AddRange(creatorMembership, removedMembership, keptMembership, addedMembership);
        context.SharedEvents.Add(sharedEvent);
        context.SharedEventParticipants.AddRange(originalParticipants);
        await context.SaveChangesAsync();

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(creator.Id);

        var handler = new UpdateSharedEventHandler(
            new SharedEventRepository(context),
            new SharedEventParticipantRepository(context),
            new GroupMemberRepository(context),
            currentUserService,
            new AuditLogRepository(context),
            new NotificationRepository(context),
            context);

        var request = new UpdateSharedEventRequest
        {
            SharedEventId = sharedEvent.Id,
            Title = "Evento atualizado",
            Description = "Descricao atualizada",
            Points = 25,
            ParticipantUserIds = new List<Guid> { keptParticipant.Id, addedParticipant.Id }
        };

        var response = await handler.HandleAsync(request, CancellationToken.None);

        response.Title.Should().Be("Evento atualizado");
        response.Points.Should().Be(25);

        var updatedSharedEvent = await context.SharedEvents
            .Include(se => se.Participants)
            .SingleAsync(se => se.Id == sharedEvent.Id);

        updatedSharedEvent.Participants.Select(p => p.UserId)
            .Should()
            .BeEquivalentTo(new[] { keptParticipant.Id, addedParticipant.Id });

        var updatedMemberships = await context.GroupMembers
            .Where(gm => gm.GroupId == group.Id)
            .ToListAsync();

        updatedMemberships.Single(gm => gm.UserId == creator.Id).CurrentScore.Should().Be(0);
        updatedMemberships.Single(gm => gm.UserId == removedParticipant.Id).CurrentScore.Should().Be(0);
        updatedMemberships.Single(gm => gm.UserId == keptParticipant.Id).CurrentScore.Should().Be(25);
        updatedMemberships.Single(gm => gm.UserId == addedParticipant.Id).CurrentScore.Should().Be(25);
    }
}
