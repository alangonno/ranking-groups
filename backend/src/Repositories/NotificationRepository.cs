using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<int> GetCountByUserIdAsync(Guid userId);
    void Add(Notification notification);
    void AddRange(IEnumerable<Notification> notifications);
    void Remove(Notification notification);
    void RemoveByUserId(Guid userId);
    void RemoveByEventId(Guid eventId);
    void RemoveBySharedEventId(Guid sharedEventId);
}

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId);
    }

    public void Add(Notification notification)
    {
        _context.Notifications.Add(notification);
    }

    public void AddRange(IEnumerable<Notification> notifications)
    {
        _context.Notifications.AddRange(notifications);
    }

    public void Remove(Notification notification)
    {
        _context.Notifications.Remove(notification);
    }

    public void RemoveByUserId(Guid userId)
    {
        var notifications = _context.Notifications.Where(n => n.UserId == userId);
        _context.Notifications.RemoveRange(notifications);
    }

    public void RemoveByEventId(Guid eventId)
    {
        var notifications = _context.Notifications.Where(n => n.EventId == eventId);
        _context.Notifications.RemoveRange(notifications);
    }

    public void RemoveBySharedEventId(Guid sharedEventId)
    {
        var notifications = _context.Notifications.Where(n => n.SharedEventId == sharedEventId);
        _context.Notifications.RemoveRange(notifications);
    }
}
