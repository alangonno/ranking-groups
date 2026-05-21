using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id);
    Task<Group?> GetByInviteCodeAsync(string inviteCode);
    Task<bool> ExistsByInviteCodeAsync(string inviteCode);
    void Add(Group group);
    void Remove(Group group);
}

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode);
    }

    public async Task<bool> ExistsByInviteCodeAsync(string inviteCode)
    {
        return await _context.Groups.AnyAsync(g => g.InviteCode == inviteCode);
    }

    public void Add(Group group)
    {
        _context.Groups.Add(group);
    }

    public void Remove(Group group)
    {
        _context.Groups.Remove(group);
    }
}
