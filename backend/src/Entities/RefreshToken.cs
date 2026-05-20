using backend.src.Entities.Base;

namespace backend.src.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
