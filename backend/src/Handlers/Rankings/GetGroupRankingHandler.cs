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
    public int Score { get; set; }
    public int Position { get; set; }
}

public interface IGetGroupRankingHandler
{
    Task<GetGroupRankingResponse> HandleAsync(GetGroupRankingRequest request, CancellationToken ct);
}

public class GetGroupRankingHandler : IGetGroupRankingHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetGroupRankingHandler(
        IEventRepository eventRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _eventRepository = eventRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetGroupRankingResponse> HandleAsync(GetGroupRankingRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupPermissionRules.ValidateUserCanInteract(userId, request.GroupId, members);

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddYears(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var events = await _eventRepository.GetByGroupAsync(request.GroupId);
        var approvedEvents = events.Where(e => e.Status == EventStatus.Approved).ToList();

        var memberScores = members.Select(m =>
        {
            var userEvents = approvedEvents.Where(e => e.AffectedUserId == m.UserId).ToList();
            var score = RankingRules.CalculateScoreFromEvents(userEvents, fromDate, toDate);

            return new RankingMemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name ?? string.Empty,
                Score = score
            };
        });

        var ranked = memberScores
            .OrderByDescending(m => m.Score)
            .Select((m, index) => new RankingMemberDto
            {
                UserId = m.UserId,
                Name = m.Name,
                Score = m.Score,
                Position = index + 1
            })
            .ToList();

        return new GetGroupRankingResponse { Members = ranked };
    }
}
