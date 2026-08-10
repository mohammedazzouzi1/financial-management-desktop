using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;

    public AuthService(IDbContextFactory<MizanDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        user.LastLoginAt = DateTime.Now;
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<User> CreateUserAsync(string username, string password, string fullName, UserRole role, string? email = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            Email = email,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> AnyUsersExistAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Users.AnyAsync();
    }

    public async Task ChangePasswordAsync(int userId, string newPassword)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user == null) return;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ModifiedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }
}
