using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;
using SkiaSharp;

namespace MizanFinance.App.ViewModels;

public partial class BankAccountsViewModel : ObservableObject
{
    private readonly IAccountService _accountService;
    private readonly Func<Account?, AccountEditViewModel> _editViewModelFactory;
    private readonly Func<Transaction?, TransactionEditViewModel> _transactionEditViewModelFactory;

    public BankAccountsViewModel(
        IAccountService accountService,
        Func<Account?, AccountEditViewModel> editViewModelFactory,
        Func<Transaction?, TransactionEditViewModel> transactionEditViewModelFactory)
    {
        _accountService = accountService;
        _editViewModelFactory = editViewModelFactory;
        _transactionEditViewModelFactory = transactionEditViewModelFactory;

        BalanceSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Name = "Solde",
                Values = Array.Empty<decimal>(),
                Fill = null,
                GeometrySize = 4,
                Stroke = new SolidColorPaint(new SKColor(0x25, 0x63, 0xEB), 3)
            }
        };
        BalanceXAxes = new Axis[] { new() { Labels = new List<string>(), TextSize = 11 } };
    }

    public ObservableCollection<Account> Accounts { get; } = new();

    [ObservableProperty] private Account? selectedAccount;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ISeries[] BalanceSeries { get; private set; }
    public Axis[] BalanceXAxes { get; private set; }

    public event Action<AccountEditViewModel>? EditRequested;
    public event Action<TransactionEditViewModel>? TransactionRequested;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var accounts = await _accountService.GetAllAsync(AccountType.Bank, includeInactive: true);
            Accounts.Clear();
            foreach (var a in accounts) Accounts.Add(a);

            SelectedAccount ??= Accounts.FirstOrDefault();
            if (SelectedAccount != null) await RefreshChartAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les comptes bancaires.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedAccountChanged(Account? value) => _ = RefreshChartAsync();

    private async Task RefreshChartAsync()
    {
        if (SelectedAccount == null) return;
        var points = await _accountService.GetBalanceEvolutionAsync(SelectedAccount.Id, DateTime.Today.AddDays(-60), DateTime.Today);
        ((LineSeries<decimal>)BalanceSeries[0]).Values = points.Select(p => p.Value).ToArray();
        BalanceXAxes[0].Labels = points.Select(p => p.Label).ToList();
        OnPropertyChanged(nameof(BalanceSeries));
        OnPropertyChanged(nameof(BalanceXAxes));
    }

    [RelayCommand]
    private void AddAccount() => EditRequested?.Invoke(_editViewModelFactory(null));

    [RelayCommand]
    private void EditAccount(Account? account)
    {
        if (account == null) return;
        EditRequested?.Invoke(_editViewModelFactory(account));
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(Account? account)
    {
        if (account == null) return;
        await _accountService.SetActiveAsync(account.Id, !account.IsActive);
        await LoadAsync();
    }

    [RelayCommand]
    private void NewOperation()
    {
        var vm = _transactionEditViewModelFactory(null);
        TransactionRequested?.Invoke(vm);
    }
}
