using backend.src.Entities;

namespace backend.tests.Builders;

public class UserBuilder
{
    private User _user = new()
    {
        Name = "Test User",
        Username = $"testuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
        Email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
        PasswordHash = "hashed_password"
    };

    public UserBuilder WithId(Guid id)
    {
        typeof(User).GetProperty("Id")?.SetValue(_user, id);
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _user.Username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _user.PasswordHash = passwordHash;
        return this;
    }

    public UserBuilder WithAvatarUrl(string? avatarUrl)
    {
        _user.AvatarUrl = avatarUrl;
        return this;
    }

    public User Build() => _user;
}
