using backend.src.Common.Exceptions;
using backend.src.Entities;
using backend.src.Entities.Enums;

namespace backend.src.Common.Rules;

public static class GroupRules
{
    public static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("group_name_required", "O nome do grupo é obrigatório.");
        }
    }

    public static void ValidateInviteCode(string inviteCode)
    {
        if (string.IsNullOrWhiteSpace(inviteCode))
        {
            throw new BusinessRuleException("invite_code_required", "O código de convite é obrigatório.");
        }
    }

    public static void ValidateNotAlreadyMember(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        var isMember = groupMembers.Any(gm => gm.UserId == userId && gm.GroupId == groupId);
        if (isMember)
        {
            throw new BusinessRuleException("already_group_member", "O usuário já é membro deste grupo.");
        }
    }

    public static void ValidateInviteCodeExists(Group? group)
    {
        if (group == null)
        {
            throw new BusinessRuleException("invalid_invite_code", "Código de convite inválido.");
        }
    }

    public static void ValidateUserIsMember(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        var isMember = groupMembers.Any(gm => gm.UserId == userId && gm.GroupId == groupId);
        if (!isMember)
        {
            throw new BusinessRuleException("user_not_in_group", "O usuário não é membro deste grupo.");
        }
    }

    public static void ValidateCanLeaveGroup(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        ValidateUserIsMember(userId, groupId, groupMembers);
    }

    public static void ValidateOwnershipTransferRequired(Guid leavingUserId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        var leavingMember = groupMembers.FirstOrDefault(gm => gm.UserId == leavingUserId && gm.GroupId == groupId);
        if (leavingMember?.Role != GroupRole.Owner)
        {
            return;
        }

        var otherMembers = groupMembers.Where(gm => gm.GroupId == groupId && gm.UserId != leavingUserId).ToList();
        if (!otherMembers.Any())
        {
            return;
        }

        var hasOtherAdminOrOwner = otherMembers.Any(gm => gm.Role == GroupRole.Admin || gm.Role == GroupRole.Owner);
        if (!hasOtherAdminOrOwner)
        {
            throw new BusinessRuleException(
                "ownership_transfer_required",
                "Você é o único owner/admin deste grupo. Transfira a ownership para outro membro antes de sair."
            );
        }
    }

    public static void ValidateOwnerCanLeaveWithTransfer(Guid leavingUserId, Guid groupId, Guid? newOwnerId, IEnumerable<GroupMember> groupMembers)
    {
        if (newOwnerId == null || newOwnerId == Guid.Empty)
        {
            throw new BusinessRuleException("new_owner_required", "É necessário informar o novo owner ao sair do grupo.");
        }

        var newOwner = groupMembers.FirstOrDefault(gm => gm.UserId == newOwnerId && gm.GroupId == groupId);
        if (newOwner == null)
        {
            throw new BusinessRuleException("new_owner_not_member", "O usuário selecionado não é membro do grupo.");
        }

        if (newOwner.UserId == leavingUserId)
        {
            throw new BusinessRuleException("cannot_transfer_to_self", "Não é possível transferir ownership para si mesmo.");
        }
    }

    public static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string NormalizeInviteCode(string inviteCode)
    {
        return inviteCode.Trim().ToUpperInvariant();
    }
}
