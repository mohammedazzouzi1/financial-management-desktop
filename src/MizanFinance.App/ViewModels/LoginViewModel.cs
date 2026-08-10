using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.App.Services;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly CurrentUserService _currentUserService;

    public LoginViewModel(IAuthService authService, CurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public event Action? LoginSucceeded;

    public Func<string>? PasswordAccessor { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Veuillez saisir votre nom d'utilisateur.";
            return;
        }

        var password = PasswordAccessor?.Invoke() ?? string.Empty;
        if (string.IsNullOrEmpty(password))
        {
            ErrorMessage = "Veuillez saisir votre mot de passe.";
            return;
        }

        IsBusy = true;
        try
        {
            var user = await _authService.LoginAsync(Username.Trim(), password);
            if (user == null)
            {
                ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
                return;
            }

            _currentUserService.SetUser(user);
            LoginSucceeded?.Invoke();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de se connecter. Vérifiez la base de données et réessayez.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
