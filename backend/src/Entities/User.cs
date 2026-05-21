using backend.src.Entities.Base;

namespace backend.src.Entities;

public class User : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    public ICollection<Event> AffectedEvents { get; set; } = new List<Event>();
    public ICollection<EventApproval> EventApprovals { get; set; } = new List<EventApproval>();
    public ICollection<SharedEvent> CreatedSharedEvents { get; set; } = new List<SharedEvent>();
    public ICollection<SharedEventParticipant> SharedEventParticipations { get; set; } = new List<SharedEventParticipant>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
