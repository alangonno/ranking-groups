using backend.src.Common.Exceptions;
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
    public string AccessToken { get; set; } = string.Empty;
}

// 3. Interface
public interface IRefreshTokenHandler
{
    Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct);
}

// 4. Implementação
public class RefreshTokenHandler : IRefreshTokenHandler
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;

    public RefreshTokenHandler(
        IJwtService jwtService,
        IUserRepository userRepository)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        RefreshTokenRequestValidator.Validate(request);

        var userId = _jwtService.ValidateRefreshToken(request.RefreshToken);
        if (userId == null)
        {
            throw new BusinessRuleException("invalid_refresh_token", "Refresh token inválido ou expirado.");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            throw new BusinessRuleException("invalid_refresh_token", "Refresh token inválido ou expirado.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Name, user.Email, user.Username);

        return new RefreshTokenResponse
        {
            AccessToken = accessToken
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
