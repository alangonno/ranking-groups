using backend.src.Entities.Base;

namespace backend.src.Entities;

public class SharedEvent : Entity
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosesAt { get; set; }
    public ICollection<SharedEventParticipant> Participants { get; set; } = new List<SharedEventParticipant>();
}
