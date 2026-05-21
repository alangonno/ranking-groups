using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class JoinGroupRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

public class JoinGroupResponse
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public interface IJoinGroupHandler
{
    Task<JoinGroupResponse> HandleAsync(JoinGroupRequest request, CancellationToken ct);
}

public class JoinGroupHandler : IJoinGroupHandler
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public JoinGroupHandler(
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService,
        AppDbContext context)
    {
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<JoinGroupResponse> HandleAsync(JoinGroupRequest request, CancellationToken ct)
    {
        JoinGroupRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var normalizedCode = GroupRules.NormalizeInviteCode(request.InviteCode);
        var group = await _groupRepository.GetByInviteCodeAsync(normalizedCode);

        GroupRules.ValidateInviteCodeExists(group);

        var existingMembers = await _groupMemberRepository.GetMembersByGroupAsync(group!.Id);
        GroupRules.ValidateNotAlreadyMember(userId, group.Id, existingMembers);

        var member = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Member,
            CurrentScore = 0
        };

        _groupMemberRepository.Add(member);
        await _context.SaveChangesAsync(ct);

        return new JoinGroupResponse
        {
            GroupId = group.Id,
            Name = group.Name,
            JoinedAt = member.CreatedAt
        };
    }
}

public static class JoinGroupRequestValidator
{
    public static void Validate(JoinGroupRequest request)
    {
        GroupRules.ValidateInviteCode(request.InviteCode);
    }
}
