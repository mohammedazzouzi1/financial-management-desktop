using MizanFinance.App.ViewModels;
using Wpf.Ui.Controls;

namespace MizanFinance.App.Views;

public partial class FirstRunWindow : FluentWindow
{
    public FirstRunWindow(FirstRunViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.AdminPasswordAccessor = () => AdminPasswordBox.Password;
        viewModel.AdminPasswordConfirmAccessor = () => AdminPasswordConfirmBox.Password;

        viewModel.SetupCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.SetupCancelled += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}
