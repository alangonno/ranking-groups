using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class GetUserGroupProfileRequest
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}

public class GetUserGroupProfileResponse
{
    public MemberProfileDto Member { get; set; } = null!;
    public List<UserEventHistoryDto> Events { get; set; } = new();
    public List<SharedEventParticipationDto> SharedEvents { get; set; } = new();
}

public class MemberProfileDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
    public int RankPosition { get; set; }
}

public class UserEventHistoryDto
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public Guid AffectedUserId { get; set; }
    public string AffectedUserName { get; set; } = string.Empty;
    public int ScoreBalance { get; set; }
}

public class SharedEventParticipationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public string UserRole { get; set; } = string.Empty;
}

public interface IGetUserGroupProfileHandler
{
    Task<GetUserGroupProfileResponse> HandleAsync(GetUserGroupProfileRequest request, CancellationToken ct);
}

public class GetUserGroupProfileHandler : IGetUserGroupProfileHandler
{
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserGroupProfileHandler(
        IGroupMemberRepository groupMemberRepository,
        IEventRepository eventRepository,
        ISharedEventRepository sharedEventRepository,
        ICurrentUserService currentUserService)
    {
        _groupMemberRepository = groupMemberRepository;
        _eventRepository = eventRepository;
        _sharedEventRepository = sharedEventRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetUserGroupProfileResponse> HandleAsync(GetUserGroupProfileRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(currentUserId, request.GroupId, members);

        var targetMember = members.FirstOrDefault(m => m.UserId == request.UserId);
        if (targetMember == null)
        {
            throw new BusinessRuleException("user_not_in_group", "O usuário não é membro deste grupo.");
        }

        var ranking = members
            .OrderByDescending(m => m.CurrentScore)
            .Select((m, index) => new { m.UserId, Position = index + 1 })
            .ToList();

        var rankPosition = ranking.First(r => r.UserId == request.UserId).Position;

        var memberDto = new MemberProfileDto
        {
            UserId = targetMember.UserId,
            Name = targetMember.User?.Name ?? string.Empty,
            Username = targetMember.User?.Username ?? string.Empty,
            Email = targetMember.User?.Email ?? string.Empty,
            AvatarUrl = targetMember.User?.AvatarUrl,
            Role = targetMember.Role.ToString(),
            CurrentScore = targetMember.CurrentScore,
            RankPosition = rankPosition
        };

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);
        var userEvents = events
            .Where(e => e.AffectedUserId == request.UserId)
            .OrderBy(e => e.CreatedAt)
            .ToList();

        var runningBalance = 0;
        var eventDtos = new List<UserEventHistoryDto>();

        foreach (var @event in userEvents)
        {
            var impact = @event.Status == EventStatus.Approved
                ? (@event.Type == EventType.Negative ? -@event.Points : @event.Points)
                : 0;

            eventDtos.Add(new UserEventHistoryDto
            {
                EventId = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                Points = @event.Points,
                Type = @event.Type.ToString(),
                Status = @event.Status.ToString(),
                CreatedAt = @event.CreatedAt,
                CreatedByUserId = @event.CreatedByUserId,
                CreatedByUserName = @event.CreatedByUser?.Name ?? string.Empty,
                AffectedUserId = @event.AffectedUserId,
                AffectedUserName = @event.AffectedUser?.Name ?? string.Empty,
                ScoreBalance = runningBalance
            });

            runningBalance += impact;
        }

        var sharedEvents = await _sharedEventRepository.GetByGroupAsync(request.GroupId);
        var participatedSharedEvents = sharedEvents
            .Where(se => se.Participants != null && se.Participants.Any(p => p.UserId == request.UserId))
            .ToList();

        var sharedEventDtos = participatedSharedEvents.Select(se => new SharedEventParticipationDto
        {
            Id = se.Id,
            Title = se.Title,
            Description = se.Description,
            Points = se.Points,
            IsClosed = se.IsClosed,
            CreatedByUserName = se.CreatedByUser?.Name ?? string.Empty,
            ParticipantCount = se.Participants?.Count ?? 0,
            UserRole = se.CreatedByUserId == request.UserId ? "organizer" : "participant"
        }).ToList();

        return new GetUserGroupProfileResponse
        {
            Member = memberDto,
            Events = eventDtos,
            SharedEvents = sharedEventDtos
        };
    }
}
