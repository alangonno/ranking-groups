using backend.src.Common.Exceptions;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Auth;

// 1. Request
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// 2. Response
public class LoginResponse
{
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

// 3. Interface
public interface ILoginHandler
{
    Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct);
}

// 4. Implementação
public class LoginHandler : ILoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        LoginRequestValidator.Validate(request);

        User? user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            user = await _userRepository.GetByUsernameAsync(request.Email);
        }

        if (user == null)
        {
            throw new BusinessRuleException("invalid_credentials", "Email ou senha incorretos.");
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new BusinessRuleException("invalid_credentials", "Email ou senha incorretos.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Name, user.Email, user.Username);

        return new LoginResponse
        {
            UserId = user.Id,
            AccessToken = accessToken,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl
        };
    }
}

// 5. Validações
public static class LoginRequestValidator
{
    public static void Validate(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BusinessRuleException("email_required", "Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("password_required", "Senha é obrigatória.");
    }
}
