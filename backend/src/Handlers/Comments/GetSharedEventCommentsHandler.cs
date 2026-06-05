using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Comments;

public class GetSharedEventCommentsRequest
{
    public Guid SharedEventId { get; set; }
    public string? Cursor { get; set; }
}

public class GetSharedEventCommentsResponse
{
    public List<CommentDto> Comments { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
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
    private readonly ISupabaseStorageService _storageService;

    public GetSharedEventCommentsHandler(
        ICommentRepository commentRepository,
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ISupabaseStorageService storageService)
    {
        _commentRepository = commentRepository;
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _storageService = storageService;
    }

    public async Task<GetSharedEventCommentsResponse> HandleAsync(GetSharedEventCommentsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId)
            ?? throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members.Items);

        var pagedComments = await _commentRepository.GetBySharedEventAsync(request.SharedEventId, request.Cursor);
        var dtos = pagedComments.Items.Select(MapToDto).ToList();

        return new GetSharedEventCommentsResponse
        {
            Comments = dtos,
            HasMore = pagedComments.HasMore,
            NextCursor = pagedComments.NextCursor
        };
    }

    private CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            UserName = comment.User?.Name ?? string.Empty,
            AvatarUrl = !string.IsNullOrWhiteSpace(comment.User?.AvatarUrl)
                ? _storageService.GetPublicUrlFromPath(comment.User?.AvatarUrl)
                : null,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies.Select(MapToDto).ToList()
        };
    }
}
