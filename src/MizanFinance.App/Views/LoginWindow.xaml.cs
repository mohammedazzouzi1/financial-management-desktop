using System.Windows;
using System.Windows.Input;
using MizanFinance.App.ViewModels;
using Wpf.Ui.Controls;

namespace MizanFinance.App.Views;

public partial class LoginWindow : FluentWindow
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.PasswordAccessor = () => PasswordBox.Password;
        viewModel.LoginSucceeded += () =>
        {
            DialogResult = true;
            Close();
        };

        Loaded += (_, _) => UsernameBox.Focus();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
        {
            vm.LoginCommand.Execute(null);
        }
    }
}
