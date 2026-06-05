using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Comments;

public class CreateCommentRequest
{
    public Guid? EventId { get; set; }
    public Guid? SharedEventId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class CreateCommentResponse
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public interface ICreateCommentHandler
{
    Task<CreateCommentResponse> HandleAsync(CreateCommentRequest request, CancellationToken ct);
}

public class CreateCommentHandler : ICreateCommentHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly AppDbContext _context;

    public CreateCommentHandler(
        ICommentRepository commentRepository,
        IEventRepository eventRepository,
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository,
        AppDbContext context)
    {
        _commentRepository = commentRepository;
        _eventRepository = eventRepository;
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
        _context = context;
    }

    public async Task<CreateCommentResponse> HandleAsync(CreateCommentRequest request, CancellationToken ct)
    {
        CreateCommentRequestValidator.Validate(request);
        CommentRules.ValidateContentNotEmpty(request.Content);
        CommentRules.ValidateEventOrSharedEventProvided(request.EventId, request.SharedEventId);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        Guid groupId;
        if (request.EventId.HasValue)
        {
            var @event = await _eventRepository.GetByIdAsync(request.EventId.Value)
                ?? throw new BusinessRuleException("event_not_found", "Evento não encontrado.");
            groupId = @event.GroupId;
        }
        else
        {
            var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId!.Value)
                ?? throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
            groupId = sharedEvent.GroupId;
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(groupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, groupId, members);

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value)
                ?? throw new BusinessRuleException("parent_comment_not_found", "Comentário raiz não encontrado.");

            CommentRules.ValidateParentCommentBelongsToSamePost(parentComment, request.EventId, request.SharedEventId);
        }

        var comment = new Comment
        {
            UserId = userId,
            EventId = request.EventId,
            SharedEventId = request.SharedEventId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim()
        };

        _commentRepository.Add(comment);
        await _context.SaveChangesAsync(ct);

        var auditLog = AuditLogBuilder.CommentCreated(comment, userId);
        _auditLogRepository.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return new CreateCommentResponse
        {
            CommentId = comment.Id,
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UserId = userId,
            UserName = comment.User?.Name ?? string.Empty
        };
    }
}

public static class CreateCommentRequestValidator
{
    public static void Validate(CreateCommentRequest request)
    {
        if (request.ParentCommentId.HasValue && request.ParentCommentId.Value == Guid.Empty)
        {
            throw new BusinessRuleException("invalid_parent_comment_id", "ID do comentário raiz inválido.");
        }
    }
}
