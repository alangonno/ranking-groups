using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Groups;

public class LeaveGroupRequest
{
    public Guid GroupId { get; set; }
    public Guid? NewOwnerId { get; set; }
}

public class LeaveGroupResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface ILeaveGroupHandler
{
    Task<LeaveGroupResponse> HandleAsync(LeaveGroupRequest request, CancellationToken ct);
}

public class LeaveGroupHandler : ILeaveGroupHandler
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _context;

    public LeaveGroupHandler(
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

    public async Task<LeaveGroupResponse> HandleAsync(LeaveGroupRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var members = await _groupMemberRepository.GetMembersByGroupAsync(request.GroupId);
        GroupRules.ValidateCanLeaveGroup(userId, request.GroupId, members);

        var leavingMember = members.FirstOrDefault(m => m.UserId == userId);
        var isOwner = leavingMember?.Role == GroupRole.Owner;
        var memberCount = members.Count();

        if (isOwner && memberCount > 1)
        {
            GroupRules.ValidateOwnershipTransferRequired(userId, request.GroupId, members);
            GroupRules.ValidateOwnerCanLeaveWithTransfer(userId, request.GroupId, request.NewOwnerId, members);

            var newOwner = members.First(m => m.UserId == request.NewOwnerId);
            newOwner.Role = GroupRole.Owner;
            _groupMemberRepository.Update(newOwner);
        }

        if (isOwner && memberCount == 1)
        {
            var group = await _groupRepository.GetByIdAsync(request.GroupId);
            if (group != null)
            {
                _groupRepository.Remove(group);
            }
        }
        else
        {
            if (leavingMember != null)
            {
                _groupMemberRepository.Remove(leavingMember);
            }
        }

        await _context.SaveChangesAsync(ct);

        var message = isOwner && memberCount == 1
            ? "Grupo deletado com sucesso."
            : "Você saiu do grupo com sucesso.";

        return new LeaveGroupResponse
        {
            Success = true,
            Message = message
        };
    }
}
