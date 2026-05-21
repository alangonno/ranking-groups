using backend.src.Data;
using backend.src.Extensions;
using backend.src.Handlers.Auth;
using backend.src.Handlers.Groups;
using backend.src.Middleware;
using backend.src.Repositories;
using backend.src.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// Auth services
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();

// Handlers
builder.Services.AddScoped<IRegisterHandler, RegisterHandler>();
builder.Services.AddScoped<ILoginHandler, LoginHandler>();
builder.Services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();
builder.Services.AddScoped<ICreateGroupHandler, CreateGroupHandler>();
builder.Services.AddScoped<IJoinGroupHandler, JoinGroupHandler>();
builder.Services.AddScoped<IListUserGroupsHandler, ListUserGroupsHandler>();
builder.Services.AddScoped<IGetGroupDetailsHandler, GetGroupDetailsHandler>();
builder.Services.AddScoped<ILeaveGroupHandler, LeaveGroupHandler>();

// Database
var connectionString = ConnectionStringBuilder.BuildFromEnvironment();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("backend");
        npgsql.MigrationsHistoryTable("__ef_migrations_history", "public");
    });
});

// JWT Authentication
var secret = GetEnvOrThrow("JWT_SECRET");
var issuer = GetEnvOrThrow("JWT_ISSUER");
var audience = GetEnvOrThrow("JWT_AUDIENCE");

builder.Services
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

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string GetEnvOrThrow(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
    return value;
}
