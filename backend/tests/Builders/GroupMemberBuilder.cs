using backend.src.Common.Enums;
using backend.src.Entities;

namespace backend.tests.Builders;

public class GroupMemberBuilder
{
    private GroupMember _groupMember = new()
    {
        Role = GroupRole.Member,
        CurrentScore = 0
    };

    public GroupMemberBuilder WithId(Guid id)
    {
        typeof(GroupMember).GetProperty("Id")?.SetValue(_groupMember, id);
        return this;
    }

    public GroupMemberBuilder WithGroupId(Guid groupId)
    {
        _groupMember.GroupId = groupId;
        return this;
    }

    public GroupMemberBuilder WithGroup(Group group)
    {
        _groupMember.Group = group;
        _groupMember.GroupId = group.Id;
        return this;
    }

    public GroupMemberBuilder WithUserId(Guid userId)
    {
        _groupMember.UserId = userId;
        return this;
    }

    public GroupMemberBuilder WithUser(User user)
    {
        _groupMember.User = user;
        _groupMember.UserId = user.Id;
        return this;
    }

    public GroupMemberBuilder WithRole(GroupRole role)
    {
        _groupMember.Role = role;
        return this;
    }

    public GroupMemberBuilder WithCurrentScore(int score)
    {
        _groupMember.CurrentScore = score;
        return this;
    }

    public GroupMember Build() => _groupMember;
}
