using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class GroupRulesTests
{
    [Fact]
    public void ValidateName_WithValidName_ShouldNotThrow()
    {
        var act = () => GroupRules.ValidateName("Meu Grupo");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateName_WithEmptyName_ShouldThrowBusinessRuleException(string? name)
    {
        var act = () => GroupRules.ValidateName(name!);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateInviteCode_WithValidCode_ShouldNotThrow()
    {
        var act = () => GroupRules.ValidateInviteCode("A1B2C3D4");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateInviteCode_WithEmptyCode_ShouldThrowBusinessRuleException(string? code)
    {
        var act = () => GroupRules.ValidateInviteCode(code!);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateNotAlreadyMember_NotMember_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>();

        var act = () => GroupRules.ValidateNotAlreadyMember(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateNotAlreadyMember_AlreadyMember_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Member }
        };

        var act = () => GroupRules.ValidateNotAlreadyMember(userId, groupId, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateInviteCodeExists_WithExistingGroup_ShouldNotThrow()
    {
        var group = new Group { Name = "Test", InviteCode = "ABC123" };

        var act = () => GroupRules.ValidateInviteCodeExists(group);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateInviteCodeExists_WithNullGroup_ShouldThrowBusinessRuleException()
    {
        var act = () => GroupRules.ValidateInviteCodeExists(null);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateUserIsMember_IsMember_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Member }
        };

        var act = () => GroupRules.ValidateUserIsMember(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateUserIsMember_NotMember_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>();

        var act = () => GroupRules.ValidateUserIsMember(userId, groupId, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateCanLeaveGroup_IsMember_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Member }
        };

        var act = () => GroupRules.ValidateCanLeaveGroup(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOwnershipTransferRequired_NotOwner_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Member }
        };

        var act = () => GroupRules.ValidateOwnershipTransferRequired(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOwnershipTransferRequired_OwnerAlone_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Owner }
        };

        var act = () => GroupRules.ValidateOwnershipTransferRequired(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOwnershipTransferRequired_OwnerWithOtherAdmin_ShouldNotThrow()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Owner },
            new() { UserId = Guid.NewGuid(), GroupId = groupId, Role = GroupRole.Admin }
        };

        var act = () => GroupRules.ValidateOwnershipTransferRequired(userId, groupId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOwnershipTransferRequired_OwnerWithOnlyMembers_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = userId, GroupId = groupId, Role = GroupRole.Owner },
            new() { UserId = Guid.NewGuid(), GroupId = groupId, Role = GroupRole.Member }
        };

        var act = () => GroupRules.ValidateOwnershipTransferRequired(userId, groupId, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateOwnerCanLeaveWithTransfer_WithValidNewOwner_ShouldNotThrow()
    {
        var leavingUserId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = leavingUserId, GroupId = groupId, Role = GroupRole.Owner },
            new() { UserId = newOwnerId, GroupId = groupId, Role = GroupRole.Admin }
        };

        var act = () => GroupRules.ValidateOwnerCanLeaveWithTransfer(leavingUserId, groupId, newOwnerId, members);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOwnerCanLeaveWithTransfer_WithNullNewOwner_ShouldThrowBusinessRuleException()
    {
        var leavingUserId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = leavingUserId, GroupId = groupId, Role = GroupRole.Owner },
            new() { UserId = Guid.NewGuid(), GroupId = groupId, Role = GroupRole.Admin }
        };

        var act = () => GroupRules.ValidateOwnerCanLeaveWithTransfer(leavingUserId, groupId, null, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateOwnerCanLeaveWithTransfer_WithNewOwnerNotMember_ShouldThrowBusinessRuleException()
    {
        var leavingUserId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = leavingUserId, GroupId = groupId, Role = GroupRole.Owner }
        };

        var act = () => GroupRules.ValidateOwnerCanLeaveWithTransfer(leavingUserId, groupId, newOwnerId, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateOwnerCanLeaveWithTransfer_WithSelfAsNewOwner_ShouldThrowBusinessRuleException()
    {
        var leavingUserId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var members = new List<GroupMember>
        {
            new() { UserId = leavingUserId, GroupId = groupId, Role = GroupRole.Owner }
        };

        var act = () => GroupRules.ValidateOwnerCanLeaveWithTransfer(leavingUserId, groupId, leavingUserId, members);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void GenerateInviteCode_ShouldReturn8Characters()
    {
        var code = GroupRules.GenerateInviteCode();
        code.Should().HaveLength(8);
    }

    [Fact]
    public void GenerateInviteCode_ShouldContainOnlyAlphanumericUppercase()
    {
        var code = GroupRules.GenerateInviteCode();
        code.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [Theory]
    [InlineData("abc123", "ABC123")]
    [InlineData("  ABC123  ", "ABC123")]
    [InlineData("a1b2C3d4", "A1B2C3D4")]
    public void NormalizeInviteCode_ShouldReturnUppercaseTrimmed(string input, string expected)
    {
        var result = GroupRules.NormalizeInviteCode(input);
        result.Should().Be(expected);
    }
}
