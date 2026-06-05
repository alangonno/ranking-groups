using backend.src.Common.Exceptions;
using backend.src.Entities;
using backend.src.Handlers.Auth;
using backend.src.Repositories;
using backend.src.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace backend.tests.Handlers.Auth;

public class RefreshTokenHandlerTests
{
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ISupabaseStorageService _storageService = Substitute.For<ISupabaseStorageService>();
    private readonly IRefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _handler = new RefreshTokenHandler(
            _jwtService,
            _userRepository,
            _storageService
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidToken_ShouldGenerateNewAccessToken()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Username = "testuser"
        };

        _jwtService.ValidateRefreshToken(request.RefreshToken).Returns(user.Id);
        _userRepository.GetByIdAsync(user.Id).Returns(user);
        _jwtService.GenerateAccessToken(user.Id, user.Name, user.Email, user.Username, Arg.Any<string?>()).Returns("new_jwt_token");

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_jwt_token");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidToken_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid_token"
        };

        _jwtService.ValidateRefreshToken(request.RefreshToken).Returns((Guid?)null);

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

        _jwtService.ValidateRefreshToken(request.RefreshToken).Returns((Guid?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowBusinessRuleException()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid_token_but_no_user"
        };

        var userId = Guid.NewGuid();

        _jwtService.ValidateRefreshToken(request.RefreshToken).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

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
