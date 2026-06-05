using backend.src.Common.Exceptions;
using backend.src.Common.Models;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Handlers.Rankings;
using backend.src.Repositories;
using backend.src.Services;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Rankings;

public class GetGroupRankingHandlerTests
{
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly ISharedEventRepository _sharedEventRepository = Substitute.For<ISharedEventRepository>();
    private readonly IGroupMemberRepository _groupMemberRepository = Substitute.For<IGroupMemberRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ISupabaseStorageService _storageService = Substitute.For<ISupabaseStorageService>();
    private readonly IGetGroupRankingHandler _handler;

    public GetGroupRankingHandlerTests()
    {
        _handler = new GetGroupRankingHandler(
            _eventRepository,
            _sharedEventRepository,
            _groupMemberRepository,
            _currentUserService,
            _storageService
        );
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnMembersWithAvatarUrl()
    {
        var currentUser = EntityFixtures.CreateUser("Current");
        var member1 = new UserBuilder()
            .WithName("Member 1")
            .WithAvatarUrl("avatars/user1.png")
            .Build();
        var member2 = new UserBuilder()
            .WithName("Member 2")
            .WithAvatarUrl("avatars/user2.png")
            .Build();
        var group = EntityFixtures.CreateGroup(currentUser);

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, currentUser),
            EntityFixtures.CreateGroupMember(group, member1),
            EntityFixtures.CreateGroupMember(group, member2)
        };

        _currentUserService.UserId.Returns(currentUser.Id);
        _groupMemberRepository.GetMembersByGroupAsync(group.Id).Returns(new CursorPagedResult<GroupMember>(members, false, null));
        _eventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<Event>(new List<Event>(), false, null));
        _sharedEventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<SharedEvent>(new List<SharedEvent>(), false, null));
        _storageService.GetPublicUrlFromPath(Arg.Any<string?>()).Returns(args =>
        {
            var path = args.Arg<string?>();
            return string.IsNullOrEmpty(path) ? "" : $"https://cdn.example.com/{path}";
        });

        var request = new GetGroupRankingRequest { GroupId = group.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Members.Should().HaveCount(3);
        result.Members[0].AvatarUrl.Should().Be("https://cdn.example.com/avatars/user1.png");
        result.Members[1].AvatarUrl.Should().Be("https://cdn.example.com/avatars/user2.png");
        result.Members[2].AvatarUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnMembersOrderedByScore()
    {
        var currentUser = EntityFixtures.CreateUser("Current");
        var member1 = EntityFixtures.CreateUser("Member 1");
        var member2 = EntityFixtures.CreateUser("Member 2");
        var group = EntityFixtures.CreateGroup(currentUser);

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, currentUser),
            EntityFixtures.CreateGroupMember(group, member1),
            EntityFixtures.CreateGroupMember(group, member2)
        };

        var events = new List<Event>
        {
            new EventBuilder()
                .WithGroup(group)
                .WithCreatedByUser(currentUser)
                .WithAffectedUser(member1)
                .WithType(EventType.Positive)
                .WithStatus(EventStatus.Approved)
                .WithPoints(100)
                .Build(),
            new EventBuilder()
                .WithGroup(group)
                .WithCreatedByUser(currentUser)
                .WithAffectedUser(member2)
                .WithType(EventType.Positive)
                .WithStatus(EventStatus.Approved)
                .WithPoints(50)
                .Build()
        };

        _currentUserService.UserId.Returns(currentUser.Id);
        _groupMemberRepository.GetMembersByGroupAsync(group.Id).Returns(new CursorPagedResult<GroupMember>(members, false, null));
        _eventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<Event>(events, false, null));
        _sharedEventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<SharedEvent>(new List<SharedEvent>(), false, null));

        var request = new GetGroupRankingRequest { GroupId = group.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Members.Should().HaveCount(3);
        result.Members[0].UserId.Should().Be(member1.Id);
        result.Members[0].Score.Should().Be(100);
        result.Members[0].Position.Should().Be(1);
        result.Members[1].UserId.Should().Be(member2.Id);
        result.Members[1].Score.Should().Be(50);
        result.Members[1].Position.Should().Be(2);
        result.Members[2].UserId.Should().Be(currentUser.Id);
        result.Members[2].Score.Should().Be(0);
        result.Members[2].Position.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WithSharedEvents_ShouldIncludeSharedEventPoints()
    {
        var currentUser = EntityFixtures.CreateUser("Current");
        var member1 = EntityFixtures.CreateUser("Member 1");
        var group = EntityFixtures.CreateGroup(currentUser);

        var members = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, currentUser),
            EntityFixtures.CreateGroupMember(group, member1)
        };

        var sharedEvent = new SharedEventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(currentUser)
            .WithPoints(25)
            .Build();

        typeof(SharedEvent).GetProperty("CreatedAt")?.SetValue(sharedEvent, DateTime.UtcNow.AddDays(-1));

        var participant = new SharedEventParticipantBuilder()
            .WithSharedEvent(sharedEvent)
            .WithUser(member1)
            .Build();

        typeof(SharedEvent).GetProperty("Participants")?.SetValue(sharedEvent, new List<SharedEventParticipant> { participant });

        _currentUserService.UserId.Returns(currentUser.Id);
        _groupMemberRepository.GetMembersByGroupAsync(group.Id).Returns(new CursorPagedResult<GroupMember>(members, false, null));
        _eventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<Event>(new List<Event>(), false, null));
        _sharedEventRepository.GetByGroupAsync(group.Id).Returns(new CursorPagedResult<SharedEvent>(new List<SharedEvent> { sharedEvent }, false, null));

        var request = new GetGroupRankingRequest { GroupId = group.Id };
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Members.Should().HaveCount(2);
        result.Members[0].UserId.Should().Be(member1.Id);
        result.Members[0].Score.Should().Be(25);
        result.Members[1].UserId.Should().Be(currentUser.Id);
        result.Members[1].Score.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithNonMember_ShouldThrowBusinessRuleException()
    {
        var outsider = EntityFixtures.CreateUser("Outsider");
        var group = EntityFixtures.CreateGroup();

        _currentUserService.UserId.Returns(outsider.Id);
        _groupMemberRepository.GetMembersByGroupAsync(group.Id).Returns(new CursorPagedResult<GroupMember>(new List<GroupMember>(), false, null));

        var request = new GetGroupRankingRequest { GroupId = group.Id };
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "user_not_in_group");
    }
}
