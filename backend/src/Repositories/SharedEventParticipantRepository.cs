using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface ISharedEventParticipantRepository
{
    Task<IEnumerable<SharedEventParticipant>> GetBySharedEventAsync(Guid sharedEventId);
    Task<SharedEventParticipant?> GetBySharedEventAndUserAsync(Guid sharedEventId, Guid userId);
    Task<bool> ExistsAsync(Guid sharedEventId, Guid userId);
    void Add(SharedEventParticipant participant);
    void Update(SharedEventParticipant participant);
    void Remove(SharedEventParticipant participant);
}

public class SharedEventParticipantRepository : ISharedEventParticipantRepository
{
    private readonly AppDbContext _context;

    public SharedEventParticipantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SharedEventParticipant>> GetBySharedEventAsync(Guid sharedEventId)
    {
        return await _context.SharedEventParticipants
            .Include(p => p.User)
            .Where(p => p.SharedEventId == sharedEventId)
            .ToListAsync();
    }

    public async Task<SharedEventParticipant?> GetBySharedEventAndUserAsync(Guid sharedEventId, Guid userId)
    {
        return await _context.SharedEventParticipants
            .FirstOrDefaultAsync(p => p.SharedEventId == sharedEventId && p.UserId == userId);
    }

    public async Task<bool> ExistsAsync(Guid sharedEventId, Guid userId)
    {
        return await _context.SharedEventParticipants
            .AnyAsync(p => p.SharedEventId == sharedEventId && p.UserId == userId);
    }

    public void Add(SharedEventParticipant participant)
    {
        _context.SharedEventParticipants.Add(participant);
    }

    public void Update(SharedEventParticipant participant)
    {
        _context.SharedEventParticipants.Update(participant);
    }

    public void Remove(SharedEventParticipant participant)
    {
        _context.SharedEventParticipants.Remove(participant);
    }
}
