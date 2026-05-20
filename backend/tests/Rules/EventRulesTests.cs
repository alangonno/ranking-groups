using backend.src.Common.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.src.Entities;
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
    public void ValidatePoints_WithNegativePoints_ShouldNotThrow()
    {
        var act = () => EventRules.ValidatePoints(-10);
        act.Should().NotThrow();
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
    public void ValidateAffectedUserIsNotCreator_WithDifferentUsers_ShouldNotThrow()
    {
        var affectedUserId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();

        var act = () => EventRules.ValidateAffectedUserIsNotCreator(affectedUserId, creatorUserId);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAffectedUserIsNotCreator_WithSameUser_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();

        var act = () => EventRules.ValidateAffectedUserIsNotCreator(userId, userId);
        act.Should().Throw<BusinessRuleException>();
    }
}
