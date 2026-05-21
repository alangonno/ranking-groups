using backend.src.Entities.Base;
using backend.src.Entities.Enums;

namespace backend.src.Entities;

public class GroupMember : Entity
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public GroupRole Role { get; set; }
    public int CurrentScore { get; set; }
}
