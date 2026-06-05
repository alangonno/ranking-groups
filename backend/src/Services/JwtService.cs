using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.src.Services;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string name, string email, string username, string? avatarUrl = null);
    string GenerateRefreshToken(Guid userId);
    Guid? ValidateRefreshToken(string token);
}

public class JwtService : IJwtService
{
    private readonly string _accessSecret;
    private readonly string _refreshSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessExpirationMinutes;
    private readonly int _refreshExpirationDays;

    public JwtService()
    {
        _accessSecret = GetEnvOrThrow("JWT_SECRET");
        _refreshSecret = GetEnvOrThrow("JWT_REFRESH_SECRET");
        _issuer = GetEnvOrThrow("JWT_ISSUER");
        _audience = GetEnvOrThrow("JWT_AUDIENCE");
        _accessExpirationMinutes = int.Parse(GetEnvOrThrow("JWT_ACCESS_EXPIRATION_MINUTES"));
        _refreshExpirationDays = int.Parse(GetEnvOrThrow("JWT_REFRESH_EXPIRATION_DAYS"));
    }

    public string GenerateAccessToken(Guid userId, string name, string email, string username, string? avatarUrl = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("name", name),
            new Claim("email", email),
            new Claim("username", username)
        };

        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            claims.Add(new Claim("avatarUrl", avatarUrl));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_refreshSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_refreshExpirationDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateRefreshToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_refreshSecret));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return null;

            return userId;
        }
        catch
        {
            return null;
        }
    }

    private static string GetEnvOrThrow(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
        return value;
    }
}
