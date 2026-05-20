using backend.src.Entities;

namespace backend.tests.Builders;

public class GroupBuilder
{
    private Group _group = new()
    {
        Name = "Test Group",
        Description = "A test group",
        InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
    };

    public GroupBuilder WithId(Guid id)
    {
        typeof(Group).GetProperty("Id")?.SetValue(_group, id);
        return this;
    }

    public GroupBuilder WithName(string name)
    {
        _group.Name = name;
        return this;
    }

    public GroupBuilder WithDescription(string? description)
    {
        _group.Description = description;
        return this;
    }

    public GroupBuilder WithInviteCode(string inviteCode)
    {
        _group.InviteCode = inviteCode;
        return this;
    }

    public GroupBuilder WithCreatedByUserId(Guid userId)
    {
        _group.CreatedByUserId = userId;
        return this;
    }

    public GroupBuilder WithCreatedByUser(User user)
    {
        _group.CreatedByUser = user;
        _group.CreatedByUserId = user.Id;
        return this;
    }

    public Group Build() => _group;
}
