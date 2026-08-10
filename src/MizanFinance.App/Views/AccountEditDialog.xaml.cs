using MizanFinance.App.ViewModels;
using Wpf.Ui.Controls;

namespace MizanFinance.App.Views;

public partial class AccountEditDialog : FluentWindow
{
    public AccountEditDialog(AccountEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.Saved += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.Cancelled += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}
