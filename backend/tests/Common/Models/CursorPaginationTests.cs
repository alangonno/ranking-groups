using backend.src.Common.Models;
using backend.src.Entities;
using backend.tests.Builders;
using FluentAssertions;
using Xunit;

namespace backend.tests.Common.Models;

public class CursorTokenTests
{
    [Fact]
    public void Encode_Should_Return_Base64_String()
    {
        var createdAt = new DateTime(2026, 6, 5, 14, 30, 0, DateTimeKind.Utc);
        var id = Guid.Parse("a1b2c3d4-e5f6-4789-8abc-def012345678");

        var result = CursorToken.Encode(createdAt, id);

        result.Should().NotBeNullOrWhiteSpace();
        // Deve ser decodificável
        var parsed = CursorToken.Parse(result);
        parsed.Should().NotBeNull();
        parsed!.CreatedAt.Should().Be(createdAt);
        parsed.Id.Should().Be(id);
    }

    [Fact]
    public void Parse_Null_Should_Return_Null()
    {
        var result = CursorToken.Parse(null);
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_Empty_Should_Return_Null()
    {
        var result = CursorToken.Parse("");
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_Invalid_Base64_Should_Return_Null()
    {
        var result = CursorToken.Parse("not-valid-base64!!!");
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_Invalid_Format_Should_Return_Null()
    {
        var invalid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("no-pipe"));
        var result = CursorToken.Parse(invalid);
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_Invalid_Date_Should_Return_Null()
    {
        var invalid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-a-date|a1b2c3d4-e5f6-4789-8abc-def012345678"));
        var result = CursorToken.Parse(invalid);
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_Invalid_Guid_Should_Return_Null()
    {
        var invalid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("2026-06-05T14:30:00Z|not-a-guid"));
        var result = CursorToken.Parse(invalid);
        result.Should().BeNull();
    }
}

public class CursorPaginationTests
{
    [Fact]
    public void Apply_Without_Cursor_Should_Return_First_Page()
    {
        var now = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
        var events = new List<Event>
        {
            new EventBuilder().WithCreatedAt(now.AddMinutes(-1)).Build(),
            new EventBuilder().WithCreatedAt(now.AddMinutes(-2)).Build(),
            new EventBuilder().WithCreatedAt(now.AddMinutes(-3)).Build(),
        }.AsQueryable();

        var result = CursorPagination.Apply(events, null, pageSize: 2);

        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeTrue();
        result.NextCursor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Apply_With_Cursor_Should_Return_Next_Page()
    {
        var now = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
        var e1 = new EventBuilder().WithCreatedAt(now.AddMinutes(-1)).Build();
        var e2 = new EventBuilder().WithCreatedAt(now.AddMinutes(-2)).Build();
        var e3 = new EventBuilder().WithCreatedAt(now.AddMinutes(-3)).Build();
        var events = new List<Event> { e1, e2, e3 }.AsQueryable();

        var firstPage = CursorPagination.Apply(events, null, pageSize: 1);
        var secondPage = CursorPagination.Apply(events, firstPage.NextCursor, pageSize: 1);

        secondPage.Items.Should().HaveCount(1);
        secondPage.Items[0].Id.Should().Be(e2.Id);
        secondPage.HasMore.Should().BeTrue();
        secondPage.NextCursor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Apply_Last_Page_Should_Have_HasMore_False()
    {
        var now = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
        var events = new List<Event>
        {
            new EventBuilder().WithCreatedAt(now.AddMinutes(-1)).Build(),
            new EventBuilder().WithCreatedAt(now.AddMinutes(-2)).Build(),
        }.AsQueryable();

        var result = CursorPagination.Apply(events, null, pageSize: 5);

        result.Items.Should().HaveCount(2);
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public void Apply_Should_Order_By_CreatedAt_Desc_Then_Id_Desc()
    {
        var now = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
        var e1 = new EventBuilder().WithCreatedAt(now.AddMinutes(-1)).Build();
        var e2 = new EventBuilder().WithCreatedAt(now.AddMinutes(-1)).Build(); // mesmo CreatedAt
        var e3 = new EventBuilder().WithCreatedAt(now.AddMinutes(-2)).Build();

        // Forçar IDs para garantir ordem previsível
        typeof(Event).GetProperty("Id")?.SetValue(e1, Guid.Parse("00000000-0000-0000-0000-000000000003"));
        typeof(Event).GetProperty("Id")?.SetValue(e2, Guid.Parse("00000000-0000-0000-0000-000000000002"));
        typeof(Event).GetProperty("Id")?.SetValue(e3, Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var events = new List<Event> { e1, e2, e3 }.AsQueryable();

        var result = CursorPagination.Apply(events, null, pageSize: 2);

        // Mesmo CreatedAt, ordem por Id descendente
        result.Items[0].Id.Should().Be(e1.Id);
        result.Items[1].Id.Should().Be(e2.Id);
    }

    [Fact]
    public void Apply_With_Same_CreatedAt_Cursor_Should_Use_Id_To_Break_Tie()
    {
        var now = new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
        var e1 = new EventBuilder().WithCreatedAt(now).Build();
        var e2 = new EventBuilder().WithCreatedAt(now).Build();
        var e3 = new EventBuilder().WithCreatedAt(now).Build();

        typeof(Event).GetProperty("Id")?.SetValue(e1, Guid.Parse("00000000-0000-0000-0000-000000000003"));
        typeof(Event).GetProperty("Id")?.SetValue(e2, Guid.Parse("00000000-0000-0000-0000-000000000002"));
        typeof(Event).GetProperty("Id")?.SetValue(e3, Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var events = new List<Event> { e1, e2, e3 }.AsQueryable();

        var firstPage = CursorPagination.Apply(events, null, pageSize: 1);
        firstPage.Items[0].Id.Should().Be(e1.Id);

        var secondPage = CursorPagination.Apply(events, firstPage.NextCursor, pageSize: 1);
        secondPage.Items[0].Id.Should().Be(e2.Id);

        var thirdPage = CursorPagination.Apply(events, secondPage.NextCursor, pageSize: 1);
        thirdPage.Items[0].Id.Should().Be(e3.Id);
        thirdPage.HasMore.Should().BeFalse();
    }
}
