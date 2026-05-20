using backend.src.Common.Enums;
using backend.src.Entities.Base;

namespace backend.src.Entities;

public class EventApproval : Entity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public EventVoteType VoteType { get; set; }
}
