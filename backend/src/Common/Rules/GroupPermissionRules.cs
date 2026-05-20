using backend.src.Common.Exceptions;
using backend.src.Entities;

namespace backend.src.Common.Rules;

public static class GroupPermissionRules
{
    public static void ValidateUserIsMember(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        var isMember = groupMembers.Any(gm => gm.UserId == userId && gm.GroupId == groupId);
        if (!isMember)
        {
            throw new BusinessRuleException(
                "user_not_in_group",
                "O usuário não é membro do grupo e não pode realizar esta ação."
            );
        }
    }

    public static void ValidateUserCanInteract(Guid userId, Guid groupId, IEnumerable<GroupMember> groupMembers)
    {
        ValidateUserIsMember(userId, groupId, groupMembers);
    }
}
