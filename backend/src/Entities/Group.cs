using backend.src.Entities.Base;

namespace backend.src.Entities;

public class Group : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<SharedEvent> SharedEvents { get; set; } = new List<SharedEvent>();
}
