using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class GroupPermissionRulesTests
{
    [Fact]
    public void ValidateUserIsMember_UserIsMember_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var member = EntityFixtures.CreateUser("Member");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, member)
        };

        var act = () => GroupPermissionRules.ValidateUserIsMember(member.Id, group.Id, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateUserIsMember_UserIsNotMember_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var outsider = EntityFixtures.CreateUser("Outsider");
        var groupMembers = new List<GroupMember>();

        var act = () => GroupPermissionRules.ValidateUserIsMember(outsider.Id, group.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateUserCanInteract_UserIsMember_ShouldNotThrow()
    {
        var group = EntityFixtures.CreateGroup();
        var member = EntityFixtures.CreateUser("Member");
        var groupMembers = new List<GroupMember>
        {
            EntityFixtures.CreateGroupMember(group, member)
        };

        var act = () => GroupPermissionRules.ValidateUserCanInteract(member.Id, group.Id, groupMembers);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateUserCanInteract_UserIsNotMember_ShouldThrowBusinessRuleException()
    {
        var group = EntityFixtures.CreateGroup();
        var outsider = EntityFixtures.CreateUser("Outsider");
        var groupMembers = new List<GroupMember>();

        var act = () => GroupPermissionRules.ValidateUserCanInteract(outsider.Id, group.Id, groupMembers);
        act.Should().Throw<BusinessRuleException>();
    }
}
