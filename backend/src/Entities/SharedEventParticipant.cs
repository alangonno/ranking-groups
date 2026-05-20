using backend.src.Entities.Base;

namespace backend.src.Entities;

public class SharedEventParticipant : Entity
{
    public Guid SharedEventId { get; set; }
    public SharedEvent SharedEvent { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
