using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.App.Services;
using MizanFinance.App.Views;

namespace MizanFinance.App.ViewModels;

public enum AppPage
{
    Dashboard,
    Transactions,
    CashRegister,
    BankAccounts,
    Settings
}

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly CurrentUserService _currentUserService;

    public MainViewModel(IServiceProvider services, CurrentUserService currentUserService)
    {
        _services = services;
        _currentUserService = currentUserService;
        NavigateTo(AppPage.Dashboard);
    }

    [ObservableProperty] private object? currentPageContent;
    [ObservableProperty] private AppPage selectedPage = AppPage.Dashboard;
    [ObservableProperty] private string currentPageTitle = "Tableau de bord";
    [ObservableProperty] private bool isPaneOpen = true;

    public string CurrentUserName => _currentUserService.User?.FullName ?? string.Empty;
    public string CurrentUserRole => _currentUserService.User?.Role.ToString() ?? string.Empty;

    public event Action? LogoutRequested;

    [RelayCommand]
    private void NavigateTo(AppPage page)
    {
        SelectedPage = page;
        CurrentPageTitle = page switch
        {
            AppPage.Dashboard => "Tableau de bord",
            AppPage.Transactions => "Transactions",
            AppPage.CashRegister => "Caisse",
            AppPage.BankAccounts => "Comptes bancaires",
            AppPage.Settings => "Paramètres",
            _ => string.Empty
        };

        CurrentPageContent = page switch
        {
            AppPage.Dashboard => _services.GetService(typeof(DashboardView)),
            AppPage.Transactions => _services.GetService(typeof(TransactionsView)),
            AppPage.CashRegister => _services.GetService(typeof(CashRegisterView)),
            AppPage.BankAccounts => _services.GetService(typeof(BankAccountsView)),
            AppPage.Settings => _services.GetService(typeof(SettingsView)),
            _ => null
        };
    }

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void Logout()
    {
        _currentUserService.Clear();
        LogoutRequested?.Invoke();
    }
}
