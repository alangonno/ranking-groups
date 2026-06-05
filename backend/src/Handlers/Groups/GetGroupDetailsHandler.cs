using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class GetGroupDetailsRequest
{
    public Guid GroupId { get; set; }
    public string? MembersCursor { get; set; }
}

public class GetGroupDetailsResponse
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public List<GroupMemberDto> Members { get; set; } = new();
    public bool MembersHasMore { get; set; }
    public string? MembersNextCursor { get; set; }
    public List<RankingDto> Ranking { get; set; } = new();
}

public class GroupMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
}

public class RankingDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Position { get; set; }
}

public interface IGetGroupDetailsHandler
{
    Task<GetGroupDetailsResponse> HandleAsync(GetGroupDetailsRequest request, CancellationToken ct);
}

public class GetGroupDetailsHandler : IGetGroupDetailsHandler
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetGroupDetailsHandler(
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetGroupDetailsResponse> HandleAsync(GetGroupDetailsRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var pagedMembers = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId, request.MembersCursor);
        var members = pagedMembers.Items;
        GroupRules.ValidateUserIsMember(userId, request.GroupId, members);

        var group = await _groupRepository.GetByIdAsync(request.GroupId);
        if (group == null)
        {
            throw new BusinessRuleException("group_not_found", "Grupo não encontrado.");
        }

        var memberDtos = members.Select(m => new GroupMemberDto
        {
            UserId = m.UserId,
            Name = m.User?.Name ?? string.Empty,
            Role = m.Role.ToString(),
            CurrentScore = m.CurrentScore
        }).ToList();

        var ranking = memberDtos
            .OrderByDescending(m => m.CurrentScore)
            .Select((m, index) => new RankingDto
            {
                UserId = m.UserId,
                Name = m.Name,
                Score = m.CurrentScore,
                Position = index + 1
            }).ToList();

        return new GetGroupDetailsResponse
        {
            GroupId = group.Id,
            Name = group.Name,
            Description = group.Description,
            InviteCode = group.InviteCode,
            Members = memberDtos,
            MembersHasMore = pagedMembers.HasMore,
            MembersNextCursor = pagedMembers.NextCursor,
            Ranking = ranking
        };
    }
}
