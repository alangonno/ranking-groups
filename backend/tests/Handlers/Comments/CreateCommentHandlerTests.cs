using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Handlers.Comments;
using backend.src.Repositories;
using backend.src.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Comments;

public class CreateCommentHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly ISharedEventRepository _sharedEventRepository = Substitute.For<ISharedEventRepository>();
    private readonly IGroupMemberRepository _groupMemberRepository = Substitute.For<IGroupMemberRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAuditLogRepository _auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly AppDbContext _context = Substitute.For<AppDbContext>();
    private readonly ICreateCommentHandler _handler;

    public CreateCommentHandlerTests()
    {
        _handler = new CreateCommentHandler(
            _commentRepository,
            _eventRepository,
            _sharedEventRepository,
            _groupMemberRepository,
            _currentUserService,
            _auditLogRepository,
            _context
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidEventComment_ShouldCreateComment()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            Content = "Great event!"
        };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId } });

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Content.Should().Be("Great event!");
        result.UserId.Should().Be(userId);
        _commentRepository.Received(1).Add(Arg.Any<Comment>());
        await _context.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        _auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WithValidSharedEventComment_ShouldCreateComment()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            SharedEventId = sharedEventId,
            Content = "Nice shared event!"
        };

        _currentUserService.UserId.Returns(userId);
        _sharedEventRepository.GetByIdAsync(sharedEventId).Returns(new SharedEvent { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId } });

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Content.Should().Be("Nice shared event!");
        result.UserId.Should().Be(userId);
        _commentRepository.Received(1).Add(Arg.Any<Comment>());
    }

    [Fact]
    public async Task HandleAsync_WithReplyToExistingComment_ShouldCreateReply()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            ParentCommentId = parentCommentId,
            Content = "Reply comment"
        };

        var parentComment = new Comment { Id = parentCommentId, EventId = eventId };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId } });
        _commentRepository.GetByIdAsync(parentCommentId).Returns(parentComment);

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.ParentCommentId.Should().Be(parentCommentId);
        _commentRepository.Received(1).Add(Arg.Any<Comment>());
    }

    [Fact]
    public async Task HandleAsync_WithEmptyContent_ShouldThrowBusinessRuleException()
    {
        var request = new CreateCommentRequest
        {
            EventId = Guid.NewGuid(),
            Content = ""
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithNeitherEventNorSharedEvent_ShouldThrowBusinessRuleException()
    {
        var request = new CreateCommentRequest
        {
            Content = "Orphan comment"
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithBothEventAndSharedEvent_ShouldThrowBusinessRuleException()
    {
        var request = new CreateCommentRequest
        {
            EventId = Guid.NewGuid(),
            SharedEventId = Guid.NewGuid(),
            Content = "Confused comment"
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentEvent_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            Content = "Comment"
        };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns((Event?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithUserNotInGroup_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            Content = "Comment"
        };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember>());

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithReplyToDifferentPost_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            ParentCommentId = parentCommentId,
            Content = "Reply"
        };

        var parentComment = new Comment { Id = parentCommentId, EventId = Guid.NewGuid() };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId } });
        _commentRepository.GetByIdAsync(parentCommentId).Returns(parentComment);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentParentComment_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            EventId = eventId,
            ParentCommentId = parentCommentId,
            Content = "Reply"
        };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId } });
        _commentRepository.GetByIdAsync(parentCommentId).Returns((Comment?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
