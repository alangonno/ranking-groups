using backend.src.Entities;

namespace backend.src.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
    void Add(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
}
