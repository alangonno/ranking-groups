using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetByGroupAsync(Guid groupId);
    void Add(Event @event);
    void Update(Event @event);
    void Remove(Event @event);
}

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.Group)
            .Include(e => e.CreatedByUser)
            .Include(e => e.AffectedUser)
            .Include(e => e.Approvals)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetByGroupAsync(Guid groupId)
    {
        return await _context.Events
            .Include(e => e.CreatedByUser)
            .Include(e => e.AffectedUser)
            .Include(e => e.Approvals)
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public void Add(Event @event)
    {
        _context.Events.Add(@event);
    }

    public void Update(Event @event)
    {
        _context.Events.Update(@event);
    }

    public void Remove(Event @event)
    {
        _context.Events.Remove(@event);
    }
}
