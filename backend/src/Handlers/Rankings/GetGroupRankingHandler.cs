using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Rankings;

public class GetGroupRankingRequest
{
    public Guid GroupId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetGroupRankingResponse
{
    public List<RankingMemberDto> Members { get; set; } = new();
}

public class RankingMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int Score { get; set; }
    public int Position { get; set; }
    public int WeeklyScore { get; set; }
}

public interface IGetGroupRankingHandler
{
    Task<GetGroupRankingResponse> HandleAsync(GetGroupRankingRequest request, CancellationToken ct);
}

public class GetGroupRankingHandler : IGetGroupRankingHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ISharedEventRepository _sharedEventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISupabaseStorageService _storageService;

    public GetGroupRankingHandler(
        IEventRepository eventRepository,
        ISharedEventRepository sharedEventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        ISupabaseStorageService storageService)
    {
        _eventRepository = eventRepository;
        _sharedEventRepository = sharedEventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _storageService = storageService;
    }

    public async Task<GetGroupRankingResponse> HandleAsync(GetGroupRankingRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var fromDate = request.FromDate;
        var toDate = request.ToDate;

        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);
        var approvedEvents = events.Where(e => e.Status == EventStatus.Approved).ToList();

        var sharedEvents = await _sharedEventRepository.GetByGroupAsync(request.GroupId);

        var memberScores = members.Select(m =>
        {
            var userEvents = approvedEvents.Where(e => e.AffectedUserId == m.UserId).ToList();
            var score = RankingRules.CalculateScoreFromEvents(userEvents, fromDate, toDate);

            var userSharedPoints = sharedEvents
                .SelectMany(se => se.Participants)
                .Where(p => p.UserId == m.UserId && (fromDate == null || p.CreatedAt >= fromDate) && (toDate == null || p.CreatedAt <= toDate))
                .Sum(p => p.SharedEvent.Points);

            score += userSharedPoints;

            var weeklyScore = RankingRules.CalculateScoreFromEvents(userEvents, weekAgo, DateTime.UtcNow);
            var weeklySharedPoints = sharedEvents
                .SelectMany(se => se.Participants)
                .Where(p => p.UserId == m.UserId && p.CreatedAt >= weekAgo)
                .Sum(p => p.SharedEvent.Points);
            weeklyScore += weeklySharedPoints;

            return new RankingMemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name ?? string.Empty,
                AvatarUrl = _storageService.GetPublicUrlFromPath(m.User?.AvatarUrl),
                Score = score,
                WeeklyScore = weeklyScore
            };
        });

        var ranked = memberScores
            .OrderByDescending(m => m.Score)
            .Select((m, index) => new RankingMemberDto
            {
                UserId = m.UserId,
                Name = m.Name,
                AvatarUrl = m.AvatarUrl,
                Score = m.Score,
                WeeklyScore = m.WeeklyScore,
                Position = index + 1
            })
            .ToList();

        return new GetGroupRankingResponse { Members = ranked };
    }
}
