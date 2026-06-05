using backend.src.Common.Models;
using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface ISharedEventRepository
{
    Task<SharedEvent?> GetByIdAsync(Guid id);
    Task<CursorPagedResult<SharedEvent>> GetByGroupAsync(Guid groupId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize);
    void Add(SharedEvent sharedEvent);
    void Update(SharedEvent sharedEvent);
    void Remove(SharedEvent sharedEvent);
}

public class SharedEventRepository : ISharedEventRepository
{
    private readonly AppDbContext _context;

    public SharedEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SharedEvent?> GetByIdAsync(Guid id)
    {
        return await _context.SharedEvents
            .Include(se => se.Group)
            .Include(se => se.CreatedByUser)
            .Include(se => se.Participants)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(se => se.Id == id);
    }

    public Task<CursorPagedResult<SharedEvent>> GetByGroupAsync(Guid groupId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize)
    {
        var query = _context.SharedEvents
            .Include(se => se.CreatedByUser)
            .Include(se => se.Participants)
            .ThenInclude(p => p.User)
            .Where(se => se.GroupId == groupId);

        return Task.FromResult(CursorPagination.Apply(query, cursor, pageSize));
    }

    public void Add(SharedEvent sharedEvent)
    {
        _context.SharedEvents.Add(sharedEvent);
    }

    public void Update(SharedEvent sharedEvent)
    {
        _context.SharedEvents.Update(sharedEvent);
    }

    public void Remove(SharedEvent sharedEvent)
    {
        _context.SharedEvents.Remove(sharedEvent);
    }
}
