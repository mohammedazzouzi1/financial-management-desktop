using System.Windows;
using System.Windows.Controls;
using MizanFinance.App.ViewModels;

namespace MizanFinance.App.Views;

public partial class CashRegisterView : UserControl
{
    public CashRegisterView(CashRegisterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CashRegisterViewModel vm && vm.LoadCommand.CanExecute(null))
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
