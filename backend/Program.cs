using backend.src.Extensions;
using backend.src.Middleware;
using DotNetEnv;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddAuthServices()
    .AddRepositories()
    .AddDatabase()
    .AddJwtAuthentication();

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
