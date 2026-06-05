using backend.src.Common.Exceptions;
using backend.src.Entities;
using backend.src.Handlers.Comments;
using backend.src.Repositories;
using backend.src.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Comments;

public class GetEventCommentsHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly IGroupMemberRepository _groupMemberRepository = Substitute.For<IGroupMemberRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IGetEventCommentsHandler _handler;

    public GetEventCommentsHandlerTests()
    {
        _handler = new GetEventCommentsHandler(
            _commentRepository,
            _eventRepository,
            _groupMemberRepository,
            _currentUserService
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldReturnComments()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new GetEventCommentsRequest { EventId = eventId };

        var comments = new List<Comment>
        {
            new()
            {
                Content = "First comment",
                UserId = userId,
                User = new User { Name = "Test User" },
                EventId = eventId,
                Replies = new List<Comment>()
            }
        };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember> { new() { UserId = userId, GroupId = groupId } });
        _commentRepository.GetByEventAsync(eventId).Returns(comments);

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Comments.Should().HaveCount(1);
        result.Comments[0].Content.Should().Be("First comment");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentEvent_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new GetEventCommentsRequest { EventId = eventId };

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
        var request = new GetEventCommentsRequest { EventId = eventId };

        _currentUserService.UserId.Returns(userId);
        _eventRepository.GetByIdAsync(eventId).Returns(new Event { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new List<GroupMember>());

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
