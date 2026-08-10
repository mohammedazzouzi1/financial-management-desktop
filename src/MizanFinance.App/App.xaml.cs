using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MizanFinance.App.Services;
using MizanFinance.App.ViewModels;
using MizanFinance.App.Views;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Interfaces;
using MizanFinance.Data;
using MizanFinance.Data.Seed;
using MizanFinance.Data.Services;
using Wpf.Ui.Appearance;

namespace MizanFinance.App;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The app moves through several dialogs (first-run wizard -> login -> main window)
        // via ShowDialog()/Show() before a "real" main window exists. With the default
        // ShutdownMode (OnLastWindowClose), the moment the login window closes there is a
        // brief instant where zero windows are open, which makes WPF auto-shutdown the
        // whole application before the next window (MainWindow) gets shown. We take
        // explicit control of shutdown instead and call Shutdown() ourselves at every
        // real exit point (see below and MainWindow.xaml.cs).
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        ApplicationThemeManager.Apply(ApplicationTheme.Light);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        try
        {
            var factory = Services.GetRequiredService<IDbContextFactory<MizanDbContext>>();
            await DbInitializer.MigrateAsync(factory);

            var settingsService = Services.GetRequiredService<ISettingsService>();
            var settings = await settingsService.GetSettingsAsync();

            if (!settings.IsFirstRunComplete)
            {
                var wizard = Services.GetRequiredService<FirstRunWindow>();
                var wizardResult = wizard.ShowDialog();
                if (wizardResult != true)
                {
                    Shutdown();
                    return;
                }
            }

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            var loginResult = loginWindow.ShowDialog();
            if (loginResult != true)
            {
                Shutdown();
                return;
            }

            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            // Now that a real main window is up, restore normal "close the app when its
            // main window closes" behavior for the rest of the session.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Impossible de démarrer l'application. Veuillez réessayer.\n\nDétails techniques : {ex.Message}",
                "Mizan Finance — Erreur de démarrage",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        var dbPath = DbPathProvider.GetDatabasePath();
        services.AddDbContextFactory<MizanDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<CurrentUserService>();

        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<ITransactionService, TransactionService>();
        services.AddSingleton<ICashRegisterService, CashRegisterService>();
        services.AddSingleton<IDashboardService, DashboardService>();
        services.AddSingleton<ICategoryService, CategoryService>();
        services.AddSingleton<IClientService, ClientService>();
        services.AddSingleton<ISupplierService, SupplierService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<FirstRunViewModel>();
        services.AddTransient<FirstRunWindow>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardView>();
        services.AddTransient<TransactionsViewModel>();
        services.AddTransient<TransactionsView>();
        services.AddTransient<CashRegisterViewModel>();
        services.AddTransient<CashRegisterView>();
        services.AddTransient<BankAccountsViewModel>();
        services.AddTransient<BankAccountsView>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsView>();

        services.AddSingleton<Func<Transaction?, TransactionEditViewModel>>(sp =>
            existing => ActivatorUtilities.CreateInstance<TransactionEditViewModel>(sp, existing));
        services.AddSingleton<Func<Account?, AccountEditViewModel>>(sp =>
            existing => ActivatorUtilities.CreateInstance<AccountEditViewModel>(sp, existing, Core.Enums.AccountType.Bank));
    }
}
