using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class ListUserGroupsResponse
{
    public List<UserGroupDto> Groups { get; set; } = new();
}

public class UserGroupDto
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
    public string InviteCode { get; set; } = string.Empty;
}

public interface IListUserGroupsHandler
{
    Task<ListUserGroupsResponse> HandleAsync(CancellationToken ct);
}

public class ListUserGroupsHandler : IListUserGroupsHandler
{
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;

    public ListUserGroupsHandler(
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ListUserGroupsResponse> HandleAsync(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return new ListUserGroupsResponse { Groups = new List<UserGroupDto>() };
        }

        var memberships = await _groupMemberRepository.GetUserMembershipsAsync(userId.Value);

        var groups = memberships.Select(m => new UserGroupDto
        {
            GroupId = m.GroupId,
            Name = m.Group?.Name ?? string.Empty,
            Role = m.Role.ToString(),
            CurrentScore = m.CurrentScore,
            InviteCode = m.Group?.InviteCode ?? string.Empty
        }).ToList();

        return new ListUserGroupsResponse { Groups = groups };
    }
}
