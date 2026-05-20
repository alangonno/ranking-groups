using backend.src.Entities;

namespace backend.src.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsEmailAsync(string email);
    Task<bool> ExistsUsernameAsync(string username);
    void Add(User user);
}
