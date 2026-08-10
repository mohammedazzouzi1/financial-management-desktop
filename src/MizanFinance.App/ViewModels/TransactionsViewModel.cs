using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly Func<Transaction?, TransactionEditViewModel> _editViewModelFactory;

    public TransactionsViewModel(
        ITransactionService transactionService,
        IAccountService accountService,
        Func<Transaction?, TransactionEditViewModel> editViewModelFactory)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _editViewModelFactory = editViewModelFactory;
    }

    public ObservableCollection<Transaction> Items { get; } = new();
    public ObservableCollection<Account> Accounts { get; } = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private Account? selectedAccountFilter;
    [ObservableProperty] private TransactionType? selectedTypeFilter;
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private Transaction? selectedTransaction;

    public List<TransactionType?> TypeFilterOptions { get; } =
        new List<TransactionType?> { null }.Concat(Enum.GetValues<TransactionType>().Cast<TransactionType?>()).ToList();

    public event Action<TransactionEditViewModel>? EditRequested;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var accounts = await _accountService.GetAllAsync();
            Accounts.Clear();
            foreach (var a in accounts) Accounts.Add(a);

            var filter = new TransactionFilter
            {
                From = FromDate,
                To = ToDate,
                AccountId = SelectedAccountFilter?.Id,
                Type = SelectedTypeFilter,
                SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                Take = 300
            };

            var (items, total) = await _transactionService.GetAsync(filter);
            Items.Clear();
            foreach (var item in items) Items.Add(item);
            TotalCount = total;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les transactions.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddTransaction()
    {
        EditRequested?.Invoke(_editViewModelFactory(null));
    }

    [RelayCommand]
    private void EditTransaction(Transaction? transaction)
    {
        if (transaction == null) return;
        EditRequested?.Invoke(_editViewModelFactory(transaction));
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(Transaction? transaction)
    {
        if (transaction == null) return;
        try
        {
            await _transactionService.DeleteAsync(transaction.Id, "system");
            await LoadAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de supprimer la transaction.";
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync() => await LoadAsync();

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedAccountFilter = null;
        SelectedTypeFilter = null;
        FromDate = null;
        ToDate = null;
        await LoadAsync();
    }
}
