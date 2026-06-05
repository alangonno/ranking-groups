using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetByEventAsync(Guid eventId);
    Task<IEnumerable<Comment>> GetBySharedEventAsync(Guid sharedEventId);
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

    public async Task<IEnumerable<Comment>> GetByEventAsync(Guid eventId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetBySharedEventAsync(Guid sharedEventId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Where(c => c.SharedEventId == sharedEventId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
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
