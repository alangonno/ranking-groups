using System.Security.Claims;

namespace backend.src.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}
