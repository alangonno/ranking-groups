using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Comments;

public class GetSharedEventCommentsRequest
{
    public Guid SharedEventId { get; set; }
}

public class GetSharedEventCommentsResponse
{
    public List<CommentDto> Comments { get; set; } = new();
}

public interface IGetSharedEventCommentsHandler
{
    Task<GetSharedEventCommentsResponse> HandleAsync(GetSharedEventCommentsRequest request, CancellationToken ct);
}

public class GetSharedEventCommentsHandler : IGetSharedEventCommentsHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSharedEventCommentsHandler(
        ICommentRepository commentRepository,
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetSharedEventCommentsResponse> HandleAsync(GetSharedEventCommentsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId)
            ?? throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members);

        var comments = await _commentRepository.GetBySharedEventAsync(request.SharedEventId);
        var dtos = comments.Select(MapToDto).ToList();

        return new GetSharedEventCommentsResponse { Comments = dtos };
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            UserName = comment.User?.Name ?? string.Empty,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies.Select(MapToDto).ToList()
        };
    }
}
