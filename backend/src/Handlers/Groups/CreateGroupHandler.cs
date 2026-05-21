using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateGroupResponse
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface ICreateGroupHandler
{
    Task<CreateGroupResponse> HandleAsync(CreateGroupRequest request, CancellationToken ct);
}

public class CreateGroupHandler : ICreateGroupHandler
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public CreateGroupHandler(
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

    public async Task<CreateGroupResponse> HandleAsync(CreateGroupRequest request, CancellationToken ct)
    {
        CreateGroupRequestValidator.Validate(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var inviteCode = GroupRules.GenerateInviteCode();
        while (await _groupRepository.ExistsByInviteCodeAsync(inviteCode))
        {
            inviteCode = GroupRules.GenerateInviteCode();
        }

        var group = new Group
        {
            Name = request.Name,
            Description = request.Description,
            InviteCode = inviteCode,
            CreatedByUserId = userId
        };

        _groupRepository.Add(group);
        await _context.SaveChangesAsync(ct);

        var ownerMember = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Owner,
            CurrentScore = 0
        };

        _groupMemberRepository.Add(ownerMember);
        await _context.SaveChangesAsync(ct);

        return new CreateGroupResponse
        {
            GroupId = group.Id,
            Name = group.Name,
            InviteCode = group.InviteCode,
            CreatedAt = group.CreatedAt
        };
    }
}

public static class CreateGroupRequestValidator
{
    public static void Validate(CreateGroupRequest request)
    {
        GroupRules.ValidateName(request.Name);
    }
}
