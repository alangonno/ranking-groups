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

public class LoginHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly AppDbContext _context = Substitute.For<AppDbContext>(new DbContextOptions<AppDbContext>());
    private readonly ILoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(
            _userRepository,
            _passwordHasher,
            _jwtService,
            _refreshTokenRepository,
            _context
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidEmail_ShouldLogin()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_password"
        };

        _userRepository.GetByEmailAsync(request.Email).Returns(user);
        _passwordHasher.Verify(request.Password, user.PasswordHash).Returns(true);
        _jwtService.GenerateToken(user.Id).Returns("jwt_token");

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.AccessToken.Should().Be("jwt_token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Name.Should().Be(user.Name);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task HandleAsync_WithValidUsername_ShouldLogin()
    {
        var request = new LoginRequest
        {
            Email = "testuser",
            Password = "password123"
        };

        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_password"
        };

        _userRepository.GetByEmailAsync(request.Email).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Email).Returns(user);
        _passwordHasher.Verify(request.Password, user.PasswordHash).Returns(true);
        _jwtService.GenerateToken(user.Id).Returns("jwt_token");

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.AccessToken.Should().Be("jwt_token");
        result.Name.Should().Be(user.Name);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidUser_ShouldThrowBusinessRuleException()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent",
            Password = "password123"
        };

        _userRepository.GetByEmailAsync(request.Email).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Email).Returns((User?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ShouldThrowBusinessRuleException()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_password"
        };

        _userRepository.GetByEmailAsync(request.Email).Returns(user);
        _passwordHasher.Verify(request.Password, user.PasswordHash).Returns(false);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyEmail_ShouldThrowBusinessRuleException()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = "password123"
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
