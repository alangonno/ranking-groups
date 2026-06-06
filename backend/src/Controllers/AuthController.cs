using backend.src.Handlers.Auth;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegisterHandler _registerHandler;
    private readonly ILoginHandler _loginHandler;
    private readonly IRefreshTokenHandler _refreshTokenHandler;
    private readonly IJwtService _jwtService;

    public AuthController(
        IRegisterHandler registerHandler,
        ILoginHandler loginHandler,
        IRefreshTokenHandler refreshTokenHandler,
        IJwtService jwtService)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await _registerHandler.HandleAsync(request, ct);
        var refreshToken = _jwtService.GenerateRefreshToken(response.UserId);
        SetRefreshTokenCookie(refreshToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _loginHandler.HandleAsync(request, ct);
        var refreshToken = _jwtService.GenerateRefreshToken(response.UserId);
        SetRefreshTokenCookie(refreshToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Forbid();
        }

        var request = new RefreshTokenRequest { RefreshToken = refreshToken };
        var response = await _refreshTokenHandler.HandleAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refresh_token");
        return Ok();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var isDevelopment = HttpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }
}
