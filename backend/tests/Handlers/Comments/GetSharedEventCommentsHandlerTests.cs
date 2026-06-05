using backend.src.Common.Exceptions;
using backend.src.Common.Models;
using backend.src.Entities;
using backend.src.Handlers.Comments;
using backend.src.Repositories;
using backend.src.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Comments;

public class GetSharedEventCommentsHandlerTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ISharedEventRepository _sharedEventRepository = Substitute.For<ISharedEventRepository>();
    private readonly IGroupMemberRepository _groupMemberRepository = Substitute.For<IGroupMemberRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IGetSharedEventCommentsHandler _handler;

    public GetSharedEventCommentsHandlerTests()
    {
        _handler = new GetSharedEventCommentsHandler(
            _commentRepository,
            _sharedEventRepository,
            _groupMemberRepository,
            _currentUserService
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidSharedEvent_ShouldReturnComments()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var request = new GetSharedEventCommentsRequest { SharedEventId = sharedEventId };

        var comments = new List<Comment>
        {
            new()
            {
                Content = "Shared event comment",
                UserId = userId,
                User = new User { Name = "Test User" },
                SharedEventId = sharedEventId,
                Replies = new List<Comment>()
            }
        };

        _currentUserService.UserId.Returns(userId);
        _sharedEventRepository.GetByIdAsync(sharedEventId).Returns(new SharedEvent { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new CursorPagedResult<GroupMember>(new List<GroupMember> { new() { UserId = userId, GroupId = groupId } }, false, null));
        _commentRepository.GetBySharedEventAsync(sharedEventId).Returns(new CursorPagedResult<Comment>(comments, false, null));

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Comments.Should().HaveCount(1);
        result.Comments[0].Content.Should().Be("Shared event comment");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentSharedEvent_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var request = new GetSharedEventCommentsRequest { SharedEventId = sharedEventId };

        _currentUserService.UserId.Returns(userId);
        _sharedEventRepository.GetByIdAsync(sharedEventId).Returns((SharedEvent?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithUserNotInGroup_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var sharedEventId = Guid.NewGuid();
        var request = new GetSharedEventCommentsRequest { SharedEventId = sharedEventId };

        _currentUserService.UserId.Returns(userId);
        _sharedEventRepository.GetByIdAsync(sharedEventId).Returns(new SharedEvent { GroupId = groupId });
        _groupMemberRepository.GetMembersByGroupAsync(groupId).Returns(new CursorPagedResult<GroupMember>(new List<GroupMember>(), false, null));

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
