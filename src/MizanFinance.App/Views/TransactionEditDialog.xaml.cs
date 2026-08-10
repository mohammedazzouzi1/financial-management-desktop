using MizanFinance.App.ViewModels;
using Wpf.Ui.Controls;

namespace MizanFinance.App.Views;

public partial class TransactionEditDialog : FluentWindow
{
    public TransactionEditDialog(TransactionEditViewModel viewModel)
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

        Loaded += async (_, _) =>
        {
            if (viewModel.LoadLookupsCommand.CanExecute(null))
            {
                await viewModel.LoadLookupsCommand.ExecuteAsync(null);
            }
        };
    }
}
