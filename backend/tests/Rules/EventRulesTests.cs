using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class EventRulesTests
{
    [Theory]
    [InlineData(EventType.Positive, EventStatus.Approved)]
    [InlineData(EventType.Negative, EventStatus.Pending)]
    public void ValidateInitialStatus_WithCorrectStatus_ShouldNotThrow(EventType type, EventStatus status)
    {
        var act = () => EventRules.ValidateInitialStatus(type, status);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(EventType.Positive, EventStatus.Pending)]
    [InlineData(EventType.Positive, EventStatus.Rejected)]
    [InlineData(EventType.Negative, EventStatus.Approved)]
    [InlineData(EventType.Negative, EventStatus.Rejected)]
    public void ValidateInitialStatus_WithIncorrectStatus_ShouldThrowBusinessRuleException(EventType type, EventStatus status)
    {
        var act = () => EventRules.ValidateInitialStatus(type, status);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidatePoints_WithPositivePoints_ShouldNotThrow()
    {
        var act = () => EventRules.ValidatePoints(10);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePoints_WithNegativePoints_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRules.ValidatePoints(-10);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidatePoints_WithZeroPoints_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRules.ValidatePoints(0);
        act.Should().Throw<BusinessRuleException>();
    }

    [Theory]
    [InlineData(EventStatus.Pending)]
    [InlineData(EventStatus.Rejected)]
    [InlineData(EventStatus.Cancelled)]
    public void ValidateCanEdit_WithNonApprovedStatus_ShouldNotThrow(EventStatus status)
    {
        var act = () => EventRules.ValidateCanEdit(status);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanEdit_WithApprovedStatus_ShouldThrowBusinessRuleException()
    {
        var act = () => EventRules.ValidateCanEdit(EventStatus.Approved);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateAffectedUserCannotModify_NegativeEvent_AffectedUser_ShouldThrowBusinessRuleException()
    {
        var affectedUserId = Guid.NewGuid();

        var act = () => EventRules.ValidateAffectedUserCannotModify(affectedUserId, affectedUserId, EventType.Negative);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateAffectedUserCannotModify_NegativeEvent_DifferentUser_ShouldNotThrow()
    {
        var affectedUserId = Guid.NewGuid();
        var modifierUserId = Guid.NewGuid();

        var act = () => EventRules.ValidateAffectedUserCannotModify(affectedUserId, modifierUserId, EventType.Negative);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAffectedUserCannotModify_PositiveEvent_AffectedUser_ShouldNotThrow()
    {
        var affectedUserId = Guid.NewGuid();

        var act = () => EventRules.ValidateAffectedUserCannotModify(affectedUserId, affectedUserId, EventType.Positive);
        act.Should().NotThrow();
    }
}
