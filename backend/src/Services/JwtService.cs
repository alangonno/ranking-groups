using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.src.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string name, string email, string username);
}

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationHours;

    public JwtService()
    {
        _secret = GetEnvOrThrow("JWT_SECRET");
        _issuer = GetEnvOrThrow("JWT_ISSUER");
        _audience = GetEnvOrThrow("JWT_AUDIENCE");
        _expirationHours = int.Parse(GetEnvOrThrow("JWT_EXPIRATION_HOURS"));
    }

    public string GenerateToken(Guid userId, string name, string email, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("name", name),
            new Claim("email", email),
            new Claim("username", username)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GetEnvOrThrow(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
        return value;
    }
}
