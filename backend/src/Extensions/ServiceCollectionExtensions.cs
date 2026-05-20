using backend.src.Data;
using backend.src.Extensions;
using backend.src.Repositories;
using backend.src.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend.src.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<Handlers.Auth.IRegisterHandler, Handlers.Auth.RegisterHandler>();
        services.AddScoped<Handlers.Auth.ILoginHandler, Handlers.Auth.LoginHandler>();
        services.AddScoped<Handlers.Auth.IRefreshTokenHandler, Handlers.Auth.RefreshTokenHandler>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var connectionString = ConnectionStringBuilder.BuildFromEnvironment();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("backend");
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "public");
            });
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var secret = GetEnvOrThrow("JWT_SECRET");
        var issuer = GetEnvOrThrow("JWT_ISSUER");
        var audience = GetEnvOrThrow("JWT_AUDIENCE");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string GetEnvOrThrow(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
        }

        return value;
    }
}
