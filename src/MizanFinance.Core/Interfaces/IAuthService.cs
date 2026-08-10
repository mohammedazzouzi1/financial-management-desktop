using MizanFinance.Core.Entities;

namespace MizanFinance.Core.Interfaces;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    Task<User> CreateUserAsync(string username, string password, string fullName, Enums.UserRole role, string? email = null);
    Task<bool> AnyUsersExistAsync();
    Task ChangePasswordAsync(int userId, string newPassword);
}
