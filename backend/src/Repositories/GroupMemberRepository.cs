using backend.src.Common.Models;
using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface IGroupMemberRepository
{
    Task<GroupMember?> GetByGroupAndUserAsync(Guid groupId, Guid userId);
    Task<CursorPagedResult<GroupMember>> GetMembersByGroupAsync(Guid groupId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize);
    Task<CursorPagedResult<GroupMember>> GetUserMembershipsAsync(Guid userId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize);
    Task<int> CountMembersAsync(Guid groupId);
    void Add(GroupMember groupMember);
    void Remove(GroupMember groupMember);
    void Update(GroupMember groupMember);
}

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly AppDbContext _context;

    public GroupMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GroupMember?> GetByGroupAndUserAsync(Guid groupId, Guid userId)
    {
        return await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
    }

    public Task<CursorPagedResult<GroupMember>> GetMembersByGroupAsync(Guid groupId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize)
    {
        var query = _context.GroupMembers
            .Include(gm => gm.User)
            .Where(gm => gm.GroupId == groupId);

        return Task.FromResult(CursorPagination.Apply(query, cursor, pageSize));
    }

    public Task<CursorPagedResult<GroupMember>> GetUserMembershipsAsync(Guid userId, string? cursor = null, int pageSize = CursorPagination.DefaultPageSize)
    {
        var query = _context.GroupMembers
            .Include(gm => gm.Group)
            .Where(gm => gm.UserId == userId);

        return Task.FromResult(CursorPagination.Apply(query, cursor, pageSize));
    }

    public async Task<int> CountMembersAsync(Guid groupId)
    {
        return await _context.GroupMembers
            .CountAsync(gm => gm.GroupId == groupId);
    }

    public void Add(GroupMember groupMember)
    {
        _context.GroupMembers.Add(groupMember);
    }

    public void Remove(GroupMember groupMember)
    {
        _context.GroupMembers.Remove(groupMember);
    }

    public void Update(GroupMember groupMember)
    {
        _context.GroupMembers.Update(groupMember);
    }
}
