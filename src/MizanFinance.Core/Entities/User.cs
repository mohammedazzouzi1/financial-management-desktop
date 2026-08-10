using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.Viewer;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
