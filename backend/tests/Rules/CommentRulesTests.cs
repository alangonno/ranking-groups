using backend.src.Entities;
using backend.src.Common.Exceptions;
using backend.src.Common.Rules;
using FluentAssertions;
using Xunit;

namespace backend.tests.Rules;

public class CommentRulesTests
{
    #region ValidateContentNotEmpty

    [Theory]
    [InlineData("Valid comment")]
    [InlineData("A")]
    [InlineData("  trimmed  ")]
    public void ValidateContentNotEmpty_WithContent_ShouldNotThrow(string content)
    {
        var act = () => CommentRules.ValidateContentNotEmpty(content);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateContentNotEmpty_WithEmptyContent_ShouldThrowBusinessRuleException(string? content)
    {
        var act = () => CommentRules.ValidateContentNotEmpty(content!);
        act.Should().Throw<BusinessRuleException>();
    }

    #endregion

    #region ValidateEventOrSharedEventProvided

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithOnlyEventId_ShouldNotThrow()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(Guid.NewGuid(), null);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithOnlySharedEventId_ShouldNotThrow()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(null, Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithNeither_ShouldThrowBusinessRuleException()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(null, null);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithBoth_ShouldThrowBusinessRuleException()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(Guid.NewGuid(), Guid.NewGuid());
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithEmptyEventId_ShouldThrowBusinessRuleException()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(Guid.Empty, null);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateEventOrSharedEventProvided_WithEmptySharedEventId_ShouldThrowBusinessRuleException()
    {
        var act = () => CommentRules.ValidateEventOrSharedEventProvided(null, Guid.Empty);
        act.Should().Throw<BusinessRuleException>();
    }

    #endregion

    #region ValidateParentCommentBelongsToSamePost

    [Fact]
    public void ValidateParentCommentBelongsToSamePost_SameEvent_ShouldNotThrow()
    {
        var eventId = Guid.NewGuid();
        var parent = new Comment { EventId = eventId };

        var act = () => CommentRules.ValidateParentCommentBelongsToSamePost(parent, eventId, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateParentCommentBelongsToSamePost_SameSharedEvent_ShouldNotThrow()
    {
        var sharedEventId = Guid.NewGuid();
        var parent = new Comment { SharedEventId = sharedEventId };

        var act = () => CommentRules.ValidateParentCommentBelongsToSamePost(parent, null, sharedEventId);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateParentCommentBelongsToSamePost_DifferentEvent_ShouldThrowBusinessRuleException()
    {
        var parentEventId = Guid.NewGuid();
        var requestEventId = Guid.NewGuid();
        var parent = new Comment { EventId = parentEventId };

        var act = () => CommentRules.ValidateParentCommentBelongsToSamePost(parent, requestEventId, null);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateParentCommentBelongsToSamePost_DifferentSharedEvent_ShouldThrowBusinessRuleException()
    {
        var parentSharedEventId = Guid.NewGuid();
        var requestSharedEventId = Guid.NewGuid();
        var parent = new Comment { SharedEventId = parentSharedEventId };

        var act = () => CommentRules.ValidateParentCommentBelongsToSamePost(parent, null, requestSharedEventId);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateParentCommentBelongsToSamePost_ParentWithoutPost_ShouldThrowBusinessRuleException()
    {
        var parent = new Comment { EventId = null, SharedEventId = null };

        var act = () => CommentRules.ValidateParentCommentBelongsToSamePost(parent, Guid.NewGuid(), null);
        act.Should().Throw<BusinessRuleException>();
    }

    #endregion
}
