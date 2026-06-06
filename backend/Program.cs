using backend.src.Data;
using backend.src.Extensions;
using backend.src.Handlers.Auth;
using backend.src.Handlers.Comments;
using backend.src.Handlers.Events;
using backend.src.Handlers.Groups;
using backend.src.Handlers.Rankings;
using backend.src.Handlers.Notifications;
using backend.src.Handlers.SharedEvents;
using backend.src.Handlers.Users;
using backend.src.Handlers.Jobs;
using backend.src.Jobs;
using backend.src.Middleware;
using backend.src.Repositories;
using backend.src.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;

if (File.Exists("../.env"))
{
    Env.Load("../.env");
}

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// Auth services
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<ISupabaseStorageService, SupabaseStorageService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventApprovalRepository, EventApprovalRepository>();
builder.Services.AddScoped<ISharedEventRepository, SharedEventRepository>();
builder.Services.AddScoped<ISharedEventParticipantRepository, SharedEventParticipantRepository>();
builder.Services.AddScoped<ISharedEventParticipantRemovalVoteRepository, SharedEventParticipantRemovalVoteRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Handlers
builder.Services.AddScoped<IRegisterHandler, RegisterHandler>();
builder.Services.AddScoped<ILoginHandler, LoginHandler>();
builder.Services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();
builder.Services.AddScoped<IUpdateAvatarHandler, UpdateAvatarHandler>();
builder.Services.AddScoped<ICreateGroupHandler, CreateGroupHandler>();
builder.Services.AddScoped<IJoinGroupHandler, JoinGroupHandler>();
builder.Services.AddScoped<IListUserGroupsHandler, ListUserGroupsHandler>();
builder.Services.AddScoped<IGetGroupDetailsHandler, GetGroupDetailsHandler>();
builder.Services.AddScoped<ILeaveGroupHandler, LeaveGroupHandler>();
builder.Services.AddScoped<IGetUserGroupProfileHandler, GetUserGroupProfileHandler>();

// Event Handlers
builder.Services.AddScoped<ICreateEventHandler, CreateEventHandler>();
builder.Services.AddScoped<IGetEventHandler, GetEventHandler>();
builder.Services.AddScoped<IListGroupEventsHandler, ListGroupEventsHandler>();
builder.Services.AddScoped<IUpdateEventHandler, UpdateEventHandler>();
builder.Services.AddScoped<IDeleteEventHandler, DeleteEventHandler>();
builder.Services.AddScoped<IVoteEventHandler, VoteEventHandler>();
builder.Services.AddScoped<IRequestEventRemovalHandler, RequestEventRemovalHandler>();
builder.Services.AddScoped<IListUserGroupEventsHandler, ListUserGroupEventsHandler>();

// Shared Event Handlers
builder.Services.AddScoped<ICreateSharedEventHandler, CreateSharedEventHandler>();
builder.Services.AddScoped<IGetSharedEventHandler, GetSharedEventHandler>();
builder.Services.AddScoped<IListGroupSharedEventsHandler, ListGroupSharedEventsHandler>();
builder.Services.AddScoped<IJoinSharedEventHandler, JoinSharedEventHandler>();
builder.Services.AddScoped<ILeaveSharedEventHandler, LeaveSharedEventHandler>();
builder.Services.AddScoped<IUpdateSharedEventHandler, UpdateSharedEventHandler>();
builder.Services.AddScoped<ICloseSharedEventHandler, CloseSharedEventHandler>();
builder.Services.AddScoped<IDeleteSharedEventHandler, DeleteSharedEventHandler>();
builder.Services.AddScoped<IRequestSharedEventParticipantRemovalHandler, RequestSharedEventParticipantRemovalHandler>();
builder.Services.AddScoped<IVoteSharedEventParticipantRemovalHandler, VoteSharedEventParticipantRemovalHandler>();

// Comment Handlers
builder.Services.AddScoped<ICreateCommentHandler, CreateCommentHandler>();
builder.Services.AddScoped<IGetEventCommentsHandler, GetEventCommentsHandler>();
builder.Services.AddScoped<IGetSharedEventCommentsHandler, GetSharedEventCommentsHandler>();

// Ranking and Feed Handlers
builder.Services.AddScoped<IGetGroupRankingHandler, GetGroupRankingHandler>();
builder.Services.AddScoped<IGetGroupFeedHandler, GetGroupFeedHandler>();

// Notification Handlers
builder.Services.AddScoped<IGetNotificationsHandler, GetNotificationsHandler>();
builder.Services.AddScoped<IMarkNotificationAsReadHandler, MarkNotificationAsReadHandler>();
builder.Services.AddScoped<IMarkAllNotificationsAsReadHandler, MarkAllNotificationsAsReadHandler>();

// Job Handlers
builder.Services.AddScoped<IResolveExpiredVotesJobHandler, ResolveExpiredVotesJobHandler>();
builder.Services.AddScoped<ICloseExpiredSharedEventsJobHandler, CloseExpiredSharedEventsJobHandler>();

// Quartz
var quartzCron = Environment.GetEnvironmentVariable("QUARTZ_CRON") ?? "0 0 0 * * ?";
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("NightlyCleanupJob");
    q.AddJob<NightlyCleanupJob>(jobKey);
    q.AddTrigger(trigger =>
    {
        trigger.ForJob(jobKey)
               .WithCronSchedule(quartzCron);
    });
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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

var frontendUrls = (Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrls)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
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
