using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.SharedEvents;

public class GetSharedEventRequest
{
    public Guid SharedEventId { get; set; }
}

public class GetSharedEventResponse
{
    public Guid SharedEventId { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? CreatedByUserAvatarUrl { get; set; }
    public List<SharedEventParticipantDto> Participants { get; set; } = new();
    public int CommentCount { get; set; }
    public string? ImageUrl { get; set; }
}

public class SharedEventParticipantDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public interface IGetSharedEventHandler
{
    Task<GetSharedEventResponse> HandleAsync(GetSharedEventRequest request, CancellationToken ct);
}

public class GetSharedEventHandler : IGetSharedEventHandler
{
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentRepository _commentRepository;
    private readonly ISupabaseStorageService _storageService;

    public GetSharedEventHandler(
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ICommentRepository commentRepository,
        ISupabaseStorageService storageService)
    {
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _commentRepository = commentRepository;
        _storageService = storageService;
    }

    public async Task<GetSharedEventResponse> HandleAsync(GetSharedEventRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var sharedEvent = await _sharedEventRepository.GetByIdAsync(request.SharedEventId);
        if (sharedEvent == null)
        {
            throw new BusinessRuleException("shared_event_not_found", "Evento compartilhado não encontrado.");
        }

        var members = await _groupMemberRepository.GetMembersByGroupAsync(sharedEvent.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, sharedEvent.GroupId, members);

        var commentCount = await _commentRepository.GetCommentCountBySharedEventAsync(sharedEvent.Id);

        return new GetSharedEventResponse
        {
            SharedEventId = sharedEvent.Id,
            GroupId = sharedEvent.GroupId,
            Title = sharedEvent.Title,
            Description = sharedEvent.Description,
            Points = sharedEvent.Points,
            IsClosed = sharedEvent.IsClosed,
            CreatedAt = sharedEvent.CreatedAt,
            CreatedByUserId = sharedEvent.CreatedByUserId,
            CreatedByUserName = sharedEvent.CreatedByUser?.Name ?? string.Empty,
            CreatedByUserAvatarUrl = !string.IsNullOrWhiteSpace(sharedEvent.CreatedByUser?.AvatarUrl)
                ? _storageService.GetPublicUrlFromPath(sharedEvent.CreatedByUser?.AvatarUrl)
                : null,
            CommentCount = commentCount,
            ImageUrl = _storageService.GetPublicUrlFromPath(sharedEvent.ImageUrl),
            Participants = sharedEvent.Participants.Select(p => new SharedEventParticipantDto
            {
                UserId = p.UserId,
                UserName = p.User?.Name ?? string.Empty,
                JoinedAt = p.CreatedAt
            }).ToList()
        };
    }
}
