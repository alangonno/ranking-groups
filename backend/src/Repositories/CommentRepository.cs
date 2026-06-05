using backend.src.Common.Models;
using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task<CursorPagedResult<Comment>> GetByEventAsync(Guid eventId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize);
    Task<CursorPagedResult<Comment>> GetBySharedEventAsync(Guid sharedEventId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize);
    Task<int> GetCommentCountByEventAsync(Guid eventId);
    Task<int> GetCommentCountBySharedEventAsync(Guid sharedEventId);
    void Add(Comment comment);
    void Update(Comment comment);
    void Remove(Comment comment);
}

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<CursorPagedResult<Comment>> GetByEventAsync(Guid eventId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.EventId == eventId);

        return Task.FromResult(CursorPagination.Apply(query, cursor, pageSize));
    }

    public Task<CursorPagedResult<Comment>> GetBySharedEventAsync(Guid sharedEventId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.SharedEventId == sharedEventId);

        return Task.FromResult(CursorPagination.Apply(query, cursor, pageSize));
    }

    public async Task<int> GetCommentCountByEventAsync(Guid eventId)
    {
        return await _context.Comments
            .CountAsync(c => c.EventId == eventId);
    }

    public async Task<int> GetCommentCountBySharedEventAsync(Guid sharedEventId)
    {
        return await _context.Comments
            .CountAsync(c => c.SharedEventId == sharedEventId);
    }

    public void Add(Comment comment)
    {
        _context.Comments.Add(comment);
    }

    public void Update(Comment comment)
    {
        _context.Comments.Update(comment);
    }

    public void Remove(Comment comment)
    {
        _context.Comments.Remove(comment);
    }
}
