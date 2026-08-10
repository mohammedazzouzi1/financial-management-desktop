using MizanFinance.Core.Entities;

namespace MizanFinance.App.Services;

public class CurrentUserService
{
    public User? User { get; private set; }

    public bool IsLoggedIn => User != null;

    public void SetUser(User user) => User = user;

    public void Clear() => User = null;
}
