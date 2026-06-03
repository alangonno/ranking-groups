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

public class RegisterHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly AppDbContext _context = Substitute.For<AppDbContext>(new DbContextOptions<AppDbContext>());
    private readonly IRegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _handler = new RegisterHandler(
            _userRepository,
            _passwordHasher,
            _jwtService,
            _context
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldRegisterUser()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        _userRepository.ExistsEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsUsernameAsync(request.Username).Returns(false);
        _passwordHasher.Hash(request.Password).Returns("hashed_password");
        _jwtService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("jwt_token");

        var result = await _handler.HandleAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().NotBe(Guid.Empty);
        result.AccessToken.Should().Be("jwt_token");
        result.Name.Should().Be(request.Name);
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);

        _userRepository.Received(1).Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEmail_ShouldThrowBusinessRuleException()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        _userRepository.ExistsEmailAsync(request.Email).Returns(true);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateUsername_ShouldThrowBusinessRuleException()
    {
        var request = new RegisterRequest
        {
            Name = "Test User",
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        _userRepository.ExistsEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsUsernameAsync(request.Username).Returns(true);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowBusinessRuleException()
    {
        var request = new RegisterRequest
        {
            Name = "",
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
