using MizanFinance.App.ViewModels;
using MizanFinance.App.Views;
using Wpf.Ui.Controls;

namespace MizanFinance.App;

public partial class MainWindow : FluentWindow
{
    private readonly IServiceProvider _services;

    public MainWindow(MainViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
        DataContext = viewModel;
        viewModel.LogoutRequested += OnLogoutRequested;
    }

    private void OnLogoutRequested()
    {
        Hide();

        var loginWindow = (LoginWindow)_services.GetService(typeof(LoginWindow))!;
        var result = loginWindow.ShowDialog();
        if (result == true)
        {
            var newMain = (MainWindow)_services.GetService(typeof(MainWindow))!;
            System.Windows.Application.Current.MainWindow = newMain;
            newMain.Show();
        }
        else
        {
            System.Windows.Application.Current.Shutdown();
        }

        Close();
    }
}
