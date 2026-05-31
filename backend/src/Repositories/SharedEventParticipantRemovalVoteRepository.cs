using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public interface ISharedEventParticipantRemovalVoteRepository
{
    Task<IEnumerable<SharedEventParticipantRemovalVote>> GetByParticipantAsync(Guid participantId);
    Task<IEnumerable<SharedEventParticipantRemovalVote>> GetBySharedEventAsync(Guid sharedEventId);
    void Add(SharedEventParticipantRemovalVote vote);
    void RemoveRange(IEnumerable<SharedEventParticipantRemovalVote> votes);
}

public class SharedEventParticipantRemovalVoteRepository : ISharedEventParticipantRemovalVoteRepository
{
    private readonly AppDbContext _context;

    public SharedEventParticipantRemovalVoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SharedEventParticipantRemovalVote>> GetByParticipantAsync(Guid participantId)
    {
        return await _context.SharedEventParticipantRemovalVotes
            .Where(v => v.ParticipantId == participantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SharedEventParticipantRemovalVote>> GetBySharedEventAsync(Guid sharedEventId)
    {
        return await _context.SharedEventParticipantRemovalVotes
            .Where(v => v.SharedEventId == sharedEventId)
            .ToListAsync();
    }

    public void Add(SharedEventParticipantRemovalVote vote)
    {
        _context.SharedEventParticipantRemovalVotes.Add(vote);
    }

    public void RemoveRange(IEnumerable<SharedEventParticipantRemovalVote> votes)
    {
        _context.SharedEventParticipantRemovalVotes.RemoveRange(votes);
    }
}
