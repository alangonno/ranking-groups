using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Handlers.Auth;
using backend.src.Repositories;
using Microsoft.EntityFrameworkCore;
using backend.src.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Auth;

public class RefreshTokenHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly AppDbContext _context = Substitute.For<AppDbContext>(new DbContextOptions<AppDbContext>());
    private readonly IRefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _handler = new RefreshTokenHandler(
            _refreshTokenRepository,
            _jwtService,
            _context
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidToken_ShouldGenerateNewTokens()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var refreshToken = new RefreshToken
        {

            UserId = Guid.NewGuid(),
            User = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Username = "testuser"
            },
            Token = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken).Returns(refreshToken);
        _jwtService.GenerateToken(refreshToken.UserId, refreshToken.User.Name, refreshToken.User.Email, refreshToken.User.Username).Returns("new_jwt_token");

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(refreshToken.UserId);
        result.Token.Should().Be("new_jwt_token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(request.RefreshToken);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidToken_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid_token"
        };

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken).Returns((RefreshToken?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithRevokedToken_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "revoked_token"
        };

        var refreshToken = new RefreshToken
        {

            UserId = Guid.NewGuid(),
            Token = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken).Returns(refreshToken);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithExpiredToken_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "expired_token"
        };

        var refreshToken = new RefreshToken
        {

            UserId = Guid.NewGuid(),
            Token = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        _refreshTokenRepository.GetByTokenAsync(request.RefreshToken).Returns(refreshToken);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyToken_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
