using backend.src.Common;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Handlers.Events;
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
    public List<TimelineItemDto> Timeline { get; set; } = new();
}

public class TimelineItemDto
{
    public Guid Id { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public Guid? AffectedUserId { get; set; }
    public string? AffectedUserName { get; set; }
    public int ScoreBalance { get; set; }
    public bool? IsClosed { get; set; }
    public int? ParticipantCount { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPendingRemoval { get; set; }
    public DateTime? RemovalVoteDeadline { get; set; }
    public int QuorumRequired { get; set; }
    public int RemoveCount { get; set; }
    public int KeepCount { get; set; }
    public List<EventApprovalSummaryDto> Approvals { get; set; } = new();
}

public class EventApprovalSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string VoteType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
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
    public string? ImageUrl { get; set; }
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
    public string? ImageUrl { get; set; }
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
    private readonly ISupabaseStorageService _storageService;
    private readonly AppDbContext _context;

    public GetUserGroupProfileHandler(
        IGroupMemberRepository groupMemberRepository,
        IEventRepository eventRepository,
        ISharedEventRepository sharedEventRepository,
        ICurrentUserService currentUserService,
        ISupabaseStorageService storageService,
        AppDbContext context)
    {
        _groupMemberRepository = groupMemberRepository;
        _eventRepository = eventRepository;
        _sharedEventRepository = sharedEventRepository;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _context = context;
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
            AvatarUrl = _storageService.GetPublicUrlFromPath(targetMember.User?.AvatarUrl),
            Role = targetMember.Role.ToString(),
            CurrentScore = targetMember.CurrentScore,
            RankPosition = rankPosition
        };

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);

        // Fallback para eventos antigos criados antes da migração de deadline
        foreach (var ev in events.Where(e => e.IsPendingRemoval && !e.RemovalVoteDeadline.HasValue))
        {
            ev.RemovalVoteDeadline = DateTime.UtcNow.AddHours(48);
        }
        await _context.SaveChangesAsync(ct);

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
                ScoreBalance = runningBalance,
                ImageUrl = _storageService.GetPublicUrlFromPath(@event.ImageUrl)
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
            UserRole = se.CreatedByUserId == request.UserId ? "organizer" : "participant",
            ImageUrl = _storageService.GetPublicUrlFromPath(se.ImageUrl)
        }).ToList();

        var totalMembers = members.Count();
        var quorum = EventRemovalRules.CalculateQuorum(totalMembers);

        var eventTimelineItems = userEvents.Select(e => new TimelineItemDto
        {
            Id = e.Id,
            ItemType = "event",
            Title = e.Title,
            Description = e.Description,
            Points = e.Points,
            Type = e.Type.ToString(),
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            CreatedByUserId = e.CreatedByUserId,
            CreatedByUserName = e.CreatedByUser?.Name ?? string.Empty,
            AffectedUserId = e.AffectedUserId,
            AffectedUserName = e.AffectedUser?.Name ?? string.Empty,
            IsClosed = null,
            ParticipantCount = null,
            ImageUrl = _storageService.GetPublicUrlFromPath(e.ImageUrl),
            IsPendingRemoval = e.IsPendingRemoval,
            RemovalVoteDeadline = e.RemovalVoteDeadline,
            QuorumRequired = quorum,
            RemoveCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Remove),
            KeepCount = e.Approvals.Count(a => a.VoteType == EventVoteType.Keep),
            Approvals = e.Approvals.Select(a => new EventApprovalSummaryDto
            {
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                VoteType = a.VoteType.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList()
        });

        var sharedTimelineItems = participatedSharedEvents.SelectMany(se =>
            se.Participants
                .Where(p => p.UserId == request.UserId)
                .Select(p => new TimelineItemDto
                {
                    Id = se.Id,
                    ItemType = "shared_event",
                    Title = se.Title,
                    Description = se.Description,
                    Points = se.Points,
                    Type = "Positive",
                    Status = "Approved",
                    CreatedAt = p.CreatedAt,
                    CreatedByUserId = se.CreatedByUserId,
                    CreatedByUserName = se.CreatedByUser?.Name ?? string.Empty,
                    AffectedUserId = null,
                    AffectedUserName = null,
                    IsClosed = se.IsClosed,
                    ParticipantCount = se.Participants?.Count ?? 0,
                    ImageUrl = _storageService.GetPublicUrlFromPath(se.ImageUrl),
                    IsPendingRemoval = false
                })
        );

        var mergedTimeline = eventTimelineItems
            .Concat(sharedTimelineItems)
            .OrderBy(i => i.CreatedAt)
            .ToList();

        var runBalance = 0;
        foreach (var item in mergedTimeline)
        {
            item.ScoreBalance = runBalance;

            var impact = item.ItemType == "event"
                ? (item.Status == "Approved"
                    ? (item.Type == "Negative" ? -item.Points : item.Points)
                    : 0)
                : item.Points;

            runBalance += impact;
        }

        return new GetUserGroupProfileResponse
        {
            Member = memberDto,
            Events = eventDtos,
            SharedEvents = sharedEventDtos,
            Timeline = mergedTimeline
        };
    }
}
