using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Auth;

// 1. Request
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

// 2. Response
public class RefreshTokenResponse
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

// 3. Interface
public interface IRefreshTokenHandler
{
    Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct);
}

// 4. Implementação
public class RefreshTokenHandler : IRefreshTokenHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        AppDbContext context)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        RefreshTokenRequestValidator.Validate(request);

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            throw new BusinessRuleException("invalid_refresh_token", "Refresh token inválido.");
        }

        if (refreshToken.IsRevoked)
        {
            throw new BusinessRuleException("revoked_refresh_token", "Refresh token foi revogado.");
        }

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("expired_refresh_token", "Refresh token expirado.");
        }

        var token = _jwtService.GenerateToken(
            refreshToken.UserId,
            refreshToken.User.Name,
            refreshToken.User.Email,
            refreshToken.User.Username
        );
        var newRefreshTokenValue = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var newRefreshToken = new backend.src.Entities.RefreshToken
        {
            UserId = refreshToken.UserId,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _refreshTokenRepository.Add(newRefreshToken);
        await _context.SaveChangesAsync(ct);

        return new RefreshTokenResponse
        {
            UserId = refreshToken.UserId,
            Token = token,
            RefreshToken = newRefreshTokenValue
        };
    }
}

// 5. Validações
public static class RefreshTokenRequestValidator
{
    public static void Validate(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new BusinessRuleException("refresh_token_required", "Refresh token é obrigatório.");
    }
}
