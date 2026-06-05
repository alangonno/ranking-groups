using backend.src.Common.Exceptions;
using backend.src.Common.Models;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Handlers.Events;
using backend.src.Repositories;
using backend.src.Services;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Events;

public class RequestEventRemovalHandlerTests
{
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAuditLogRepository _auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly AppDbContext _context = InMemoryDbContextFactory.Create();
    private readonly IRequestEventRemovalHandler _handler;

    public RequestEventRemovalHandlerTests()
    {
        var eventApprovalRepository = new EventApprovalRepository(_context);
        var groupMemberRepository = new GroupMemberRepository(_context);
        _handler = new RequestEventRemovalHandler(
            _eventRepository,
            eventApprovalRepository,
            groupMemberRepository,
            _currentUserService,
            _auditLogRepository,
            _notificationRepository,
            _context
        );
    }

    private void SeedMembers(Group group, List<GroupMember> members)
    {
        _context.Groups.Add(group);
        foreach (var member in members)
        {
            _context.Users.Add(member.User!);
            _context.GroupMembers.Add(member);
        }
        _context.SaveChanges();
    }

    [Fact]
    public async Task HandleAsync_CreatorInitiatingRemoval_ShouldVoteRemove()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var otherMember = EntityFixtures.CreateUser("Other");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, otherMember)
        };

        _currentUserService.UserId.Returns(creator.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // 3 membros → quorum=1. Criador vota Remove (1 voto) → atinge quorum → remove imediatamente
        result.Should().NotBeNull();
        result.EventId.Should().Be(@event.Id);
        result.IsPendingRemoval.Should().BeFalse();
        result.RemoveCount.Should().Be(1);
        result.KeepCount.Should().Be(0);
        result.QuorumRequired.Should().Be(1);
        result.RemovedImmediately.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_OtherMemberInitiatingRemoval_ShouldCreateKeepAndRemoveVotes()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var initiator = EntityFixtures.CreateUser("Initiator");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, initiator)
        };

        _currentUserService.UserId.Returns(initiator.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.EventId.Should().Be(@event.Id);
        result.IsPendingRemoval.Should().BeTrue();
        result.RemoveCount.Should().Be(1);
        result.KeepCount.Should().Be(1);
        result.QuorumRequired.Should().Be(1);
        result.RemovedImmediately.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_BypassRemoval_ShouldRemoveImmediately()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };

        _currentUserService.UserId.Returns(affectedUser.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.EventId.Should().Be(@event.Id);
        result.IsPendingRemoval.Should().BeFalse();
        result.RemovedImmediately.Should().BeTrue();
        result.RemoveCount.Should().Be(1);
        result.KeepCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_CreatorInitiatingRemovalWithoutQuorum_ShouldOpenVoting()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var member1 = EntityFixtures.CreateUser("Member1");
        var member2 = EntityFixtures.CreateUser("Member2");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser),
            EntityFixtures.CreateGroupMember(group, member1),
            EntityFixtures.CreateGroupMember(group, member2)
        };

        _currentUserService.UserId.Returns(creator.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // 4 membros → quorum=2. Criador vota Remove (1 voto) → não atinge quorum → votação continua aberta
        result.Should().NotBeNull();
        result.EventId.Should().Be(@event.Id);
        result.IsPendingRemoval.Should().BeTrue();
        result.RemovedImmediately.Should().BeFalse();
        result.RemoveCount.Should().Be(1);
        result.KeepCount.Should().Be(0);
        result.QuorumRequired.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_PendingEvent_ShouldThrowBusinessRuleException()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Pending)
            .WithPoints(10)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };

        _currentUserService.UserId.Returns(creator.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "event_not_approved");
    }

    [Fact]
    public async Task HandleAsync_AlreadyPendingRemoval_ShouldThrowBusinessRuleException()
    {
        var creator = EntityFixtures.CreateUser("Creator");
        var affectedUser = EntityFixtures.CreateUser("Affected");
        var group = EntityFixtures.CreateGroup(creator);

        var @event = new EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(creator)
            .WithAffectedUser(affectedUser)
            .WithType(EventType.Positive)
            .WithStatus(EventStatus.Approved)
            .WithPoints(10)
            .WithIsPendingRemoval(true)
            .Build();

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, creator),
            EntityFixtures.CreateGroupMember(group, affectedUser)
        };

        _currentUserService.UserId.Returns(creator.Id);
        _eventRepository.GetByIdAsync(@event.Id).Returns(@event);
        SeedMembers(group, members);

        var request = new RequestEventRemovalRequest { EventId = @event.Id };
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "event_already_pending_removal");
    }

    [Fact]
    public void Debug_QuorumCalculation()
    {
        var members = new List<GroupMember>
        {
            new GroupMember(),
            new GroupMember(),
            new GroupMember()
        };

        var paged = new CursorPagedResult<GroupMember>(members, false, null);
        paged.Count.Should().Be(3);
        var quorum = EventRemovalRules.CalculateQuorum(paged.Count());
        quorum.Should().Be(1);
    }
}
