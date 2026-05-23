using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface IEventApprovalRepository
{
    Task<IEnumerable<EventApproval>> GetByEventAsync(Guid eventId);
    Task<bool> ExistsByEventAndUserAsync(Guid eventId, Guid userId);
    void Add(EventApproval approval);
}

public class EventApprovalRepository : IEventApprovalRepository
{
    private readonly AppDbContext _context;

    public EventApprovalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventApproval>> GetByEventAsync(Guid eventId)
    {
        return await _context.EventApprovals
            .Where(a => a.EventId == eventId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByEventAndUserAsync(Guid eventId, Guid userId)
    {
        return await _context.EventApprovals
            .AnyAsync(a => a.EventId == eventId && a.UserId == userId);
    }

    public void Add(EventApproval approval)
    {
        _context.EventApprovals.Add(approval);
    }
}
