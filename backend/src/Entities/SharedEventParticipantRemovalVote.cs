using backend.src.Entities.Base;
using backend.src.Entities.Enums;

namespace backend.src.Entities;

public class SharedEventParticipantRemovalVote : Entity
{
    public Guid SharedEventId { get; set; }
    public SharedEvent SharedEvent { get; set; } = null!;
    public Guid ParticipantId { get; set; }
    public SharedEventParticipant Participant { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public EventVoteType VoteType { get; set; }
}
