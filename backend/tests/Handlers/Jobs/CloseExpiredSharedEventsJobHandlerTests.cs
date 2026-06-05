using backend.src.Data;
using backend.src.Entities;
using backend.src.Handlers.Jobs;
using backend.src.Repositories;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.tests.Handlers.Jobs;

public class CloseExpiredSharedEventsJobHandlerTests
{
    [Fact]
    public async Task ProcessAsync_WithExpiredSharedEvent_ShouldCloseAndGenerateAuditLog()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var member = EntityFixtures.CreateGroupMember(group, creator);
        context.Groups.Add(group);
        context.Users.Add(creator);
        context.GroupMembers.Add(member);

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithClosesAt(DateTime.UtcNow.AddDays(-1))
            .Build();
        context.SharedEvents.Add(sharedEvent);
        await context.SaveChangesAsync();

        var handler = new CloseExpiredSharedEventsJobHandler(
            new SharedEventRepository(context),
            new GroupMemberRepository(context),
            new AuditLogRepository(context),
            new NotificationRepository(context),
            context
        );

        var result = await handler.ProcessAsync(DateTime.UtcNow.Date, CancellationToken.None);

        result.ClosedCount.Should().Be(1);
        var updated = await context.SharedEvents.FirstAsync();
        updated.IsClosed.Should().BeTrue();
        context.AuditLogs.Should().ContainSingle(a => a.Action == "shared_event_closed");
        context.Notifications.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_WithFutureClosesAt_ShouldNotClose()
    {
        var context = InMemoryDbContextFactory.Create();
        var group = EntityFixtures.CreateGroup();
        var creator = group.CreatedByUser;
        var member = EntityFixtures.CreateGroupMember(group, creator);
        context.Groups.Add(group);
        context.Users.Add(creator);
        context.GroupMembers.Add(member);

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithClosesAt(DateTime.UtcNow.AddDays(1))
            .Build();
        context.SharedEvents.Add(sharedEvent);
        await context.SaveChangesAsync();

        var handler = new CloseExpiredSharedEventsJobHandler(
            new SharedEventRepository(context),
            new GroupMemberRepository(context),
            new AuditLogRepository(context),
            new NotificationRepository(context),
            context
        );

        var result = await handler.ProcessAsync(DateTime.UtcNow.Date, CancellationToken.None);

        result.ClosedCount.Should().Be(0);
        var updated = await context.SharedEvents.FirstAsync();
        updated.IsClosed.Should().BeFalse();
    }
}
