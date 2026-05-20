using backend.src.Common.Enums;
using backend.src.Entities.Base;

namespace backend.src.Entities;

public class Event : Entity
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public Guid AffectedUserId { get; set; }
    public User AffectedUser { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public EventType Type { get; set; }
    public EventStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public ICollection<EventApproval> Approvals { get; set; } = new List<EventApproval>();
}
