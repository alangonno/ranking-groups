using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Comments;

public class GetEventCommentsRequest
{
    public Guid EventId { get; set; }
}

public class GetEventCommentsResponse
{
    public List<CommentDto> Comments { get; set; } = new();
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public interface IGetEventCommentsHandler
{
    Task<GetEventCommentsResponse> HandleAsync(GetEventCommentsRequest request, CancellationToken ct);
}

public class GetEventCommentsHandler : IGetEventCommentsHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetEventCommentsHandler(
        ICommentRepository commentRepository,
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetEventCommentsResponse> HandleAsync(GetEventCommentsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId)
            ?? throw new BusinessRuleException("event_not_found", "Evento não encontrado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(@event.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, @event.GroupId, members);

        var comments = await _commentRepository.GetByEventAsync(request.EventId);
        var dtos = comments.Select(MapToDto).ToList();

        return new GetEventCommentsResponse { Comments = dtos };
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
