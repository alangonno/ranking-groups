using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Auth;

// 1. Request
public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// 2. Response
public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

// 3. Interface
public interface IRegisterHandler
{
    Task<RegisterResponse> HandleAsync(RegisterRequest request, CancellationToken ct);
}

// 4. Implementação
public class RegisterHandler : IRegisterHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly AppDbContext _context;

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        AppDbContext context)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
    }

    public async Task<RegisterResponse> HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        RegisterRequestValidator.Validate(request);

        var emailExists = await _userRepository.ExistsEmailAsync(request.Email);
        if (emailExists)
        {
            throw new BusinessRuleException("email_in_use", "Email já está em uso.");
        }

        var usernameExists = await _userRepository.ExistsUsernameAsync(request.Username);
        if (usernameExists)
        {
            throw new BusinessRuleException("username_in_use", "Username já está em uso.");
        }

        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _userRepository.Add(user);
        await _context.SaveChangesAsync(ct);

        var token = _jwtService.GenerateToken(user.Id, user.Name, user.Email, user.Username);
        var refreshTokenValue = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _refreshTokenRepository.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        return new RegisterResponse
        {
            UserId = user.Id,
            AccessToken = token,
            RefreshToken = refreshTokenValue,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl
        };
    }
}

// 5. Validações
public static class RegisterRequestValidator
{
    public static void Validate(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessRuleException("name_required", "Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BusinessRuleException("username_required", "Username é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BusinessRuleException("email_required", "Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BusinessRuleException("password_required", "Senha é obrigatória.");
    }
}
