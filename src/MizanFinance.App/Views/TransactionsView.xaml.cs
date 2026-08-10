using System.Windows;
using System.Windows.Controls;
using MizanFinance.App.ViewModels;

namespace MizanFinance.App.Views;

public partial class TransactionsView : UserControl
{
    public TransactionsView(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.EditRequested += OnEditRequested;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TransactionsViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }

    private void OnEditRequested(TransactionEditViewModel editViewModel)
    {
        var dialog = new TransactionEditDialog(editViewModel)
        {
            Owner = Window.GetWindow(this)
        };

        var result = dialog.ShowDialog();
        if (result == true && DataContext is TransactionsViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
