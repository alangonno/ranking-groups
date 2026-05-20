using backend.src.Entities.Base;

namespace backend.src.Entities;

public class Notification : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
