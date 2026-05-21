using backend.src.Entities;
using backend.src.Entities.Enums;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using backend.tests.Builders;
using backend.tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class RankingRulesTests
{
    [Fact]
    public void CalculateScoreFromEvents_WithOnlyApprovedEvents_ShouldSumCorrectly()
    {
        var user = EntityFixtures.CreateUser("User");
        var group = EntityFixtures.CreateGroup();
        var otherUser = EntityFixtures.CreateUser("Other");

        var events = new List<Event>
        {
            EntityFixtures.CreatePositiveEvent(group, otherUser, user),
            new EventBuilder()
                .WithGroup(group)
                .WithCreatedByUser(otherUser)
                .WithAffectedUser(user)
                .WithType(EventType.Negative)
                .WithStatus(EventStatus.Approved)
                .WithPoints(15)
                .Build()
        };

        var score = RankingRules.CalculateScoreFromEvents(events);

        score.Should().Be(-5);
    }

    [Fact]
    public void CalculateScoreFromEvents_WithPendingEvent_ShouldIgnorePending()
    {
        var user = EntityFixtures.CreateUser("User");
        var group = EntityFixtures.CreateGroup();
        var otherUser = EntityFixtures.CreateUser("Other");

        var approvedEvent = EntityFixtures.CreatePositiveEvent(group, otherUser, user);
        var pendingEvent = new backend.tests.Builders.EventBuilder()
            .WithGroup(group)
            .WithCreatedByUser(otherUser)
            .WithAffectedUser(user)
            .WithType(EventType.Negative)
            .WithStatus(EventStatus.Pending)
            .WithPoints(20)
            .Build();

        var events = new List<Event> { approvedEvent, pendingEvent };
        var score = RankingRules.CalculateScoreFromEvents(events);

        score.Should().Be(10);
    }

    [Fact]
    public void CalculateScoreFromEvents_WithDateRange_ShouldFilterByDate()
    {
        var user = EntityFixtures.CreateUser("User");
        var group = EntityFixtures.CreateGroup();
        var otherUser = EntityFixtures.CreateUser("Other");

        var oldEvent = EntityFixtures.CreatePositiveEvent(group, otherUser, user);
        typeof(backend.src.Entities.Event).GetProperty("CreatedAt")?.SetValue(oldEvent, DateTime.UtcNow.AddDays(-400));

        var recentEvent = EntityFixtures.CreatePositiveEvent(group, otherUser, user);

        var events = new List<Event> { oldEvent, recentEvent };
        var fromDate = DateTime.UtcNow.AddDays(-365);

        var score = RankingRules.CalculateScoreFromEvents(events, fromDate);

        score.Should().Be(10);
    }

    [Fact]
    public void CalculateScoreFromEvents_WithNoEvents_ShouldReturnZero()
    {
        var events = new List<Event>();
        var score = RankingRules.CalculateScoreFromEvents(events);

        score.Should().Be(0);
    }

    [Fact]
    public void ValidateEventApprovedBeforeScoring_ApprovedEvent_ShouldNotThrow()
    {
        var act = () => RankingRules.ValidateEventApprovedBeforeScoring(EventStatus.Approved);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(EventStatus.Pending)]
    [InlineData(EventStatus.Rejected)]
    [InlineData(EventStatus.Cancelled)]
    public void ValidateEventApprovedBeforeScoring_NonApprovedEvent_ShouldThrowBusinessRuleException(EventStatus status)
    {
        var act = () => RankingRules.ValidateEventApprovedBeforeScoring(status);
        act.Should().Throw<BusinessRuleException>();
    }
}
