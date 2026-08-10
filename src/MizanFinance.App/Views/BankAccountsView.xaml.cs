using System.Windows;
using System.Windows.Controls;
using MizanFinance.App.ViewModels;

namespace MizanFinance.App.Views;

public partial class BankAccountsView : UserControl
{
    public BankAccountsView(BankAccountsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.EditRequested += OnEditRequested;
        viewModel.TransactionRequested += OnTransactionRequested;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is BankAccountsViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }

    private void OnEditRequested(AccountEditViewModel editViewModel)
    {
        var dialog = new AccountEditDialog(editViewModel) { Owner = Window.GetWindow(this) };
        var result = dialog.ShowDialog();
        if (result == true && DataContext is BankAccountsViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }

    private void OnTransactionRequested(TransactionEditViewModel editViewModel)
    {
        var dialog = new TransactionEditDialog(editViewModel) { Owner = Window.GetWindow(this) };
        var result = dialog.ShowDialog();
        if (result == true && DataContext is BankAccountsViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
