using System.Collections;
using System.Text;
using backend.src.Entities.Base;

namespace backend.src.Common.Models;

public static class CursorPagination
{
    public const int DefaultPageSize = 20;

    public static CursorPagedResult<T> Apply<T>(IQueryable<T> query, string? cursor, int pageSize = DefaultPageSize) where T : Entity
    {
        var parsedCursor = CursorToken.Parse(cursor);
        var takeCount = pageSize + 1;

        IQueryable<T> orderedQuery = query.OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id);

        if (parsedCursor != null)
        {
            orderedQuery = orderedQuery.Where(e =>
                e.CreatedAt < parsedCursor.CreatedAt ||
                (e.CreatedAt == parsedCursor.CreatedAt && e.Id.CompareTo(parsedCursor.Id) < 0));
        }

        var items = orderedQuery.Take(takeCount).ToList();
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = CursorToken.Encode(last.CreatedAt, last.Id);
        }

        return new CursorPagedResult<T>(items, hasMore, nextCursor);
    }
}

public class CursorPagedResult<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T> _items;
    public IReadOnlyList<T> Items => _items;
    public bool HasMore { get; }
    public string? NextCursor { get; }

    public CursorPagedResult(IReadOnlyList<T> items, bool hasMore, string? nextCursor)
    {
        _items = items;
        HasMore = hasMore;
        NextCursor = nextCursor;
    }

    public T this[int index] => _items[index];
    public int Count => _items.Count;
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}

public class CursorPagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public bool HasMore { get; set; }
    public string? NextCursor { get; set; }
}

public static class CursorToken
{
    public static string Encode(DateTime createdAt, Guid id)
    {
        var value = $"{createdAt:O}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static CursorValue? Parse(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split('|', 2);
            if (parts.Length != 2)
                return null;

            if (!DateTime.TryParse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdAt))
                return null;

            // Garantir UTC para evitar problemas de timezone
            if (createdAt.Kind == DateTimeKind.Local)
                createdAt = createdAt.ToUniversalTime();
            else if (createdAt.Kind == DateTimeKind.Unspecified)
                createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);

            if (!Guid.TryParse(parts[1], out var id))
                return null;

            return new CursorValue(createdAt, id);
        }
        catch
        {
            return null;
        }
    }
}

public class CursorValue
{
    public DateTime CreatedAt { get; }
    public Guid Id { get; }

    public CursorValue(DateTime createdAt, Guid id)
    {
        CreatedAt = createdAt;
        Id = id;
    }
}
