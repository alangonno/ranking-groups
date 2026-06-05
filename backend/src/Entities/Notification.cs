using backend.src.Entities.Base;

namespace backend.src.Entities;

public class Notification : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? EventId { get; set; }
    public Guid? SharedEventId { get; set; }
}
