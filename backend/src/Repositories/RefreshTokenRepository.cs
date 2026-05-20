using backend.src.Data;
using backend.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.src.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);
    }
}
