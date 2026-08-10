using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.App.Services;
using MizanFinance.Core.Dto;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class CashRegisterViewModel : ObservableObject
{
    private readonly ICashRegisterService _cashRegisterService;
    private readonly IAccountService _accountService;
    private readonly CurrentUserService _currentUserService;

    public CashRegisterViewModel(
        ICashRegisterService cashRegisterService,
        IAccountService accountService,
        CurrentUserService currentUserService)
    {
        _cashRegisterService = cashRegisterService;
        _accountService = accountService;
        _currentUserService = currentUserService;
    }

    public ObservableCollection<Account> CashAccounts { get; } = new();
    public ObservableCollection<CashRegisterSummary> History { get; } = new();

    [ObservableProperty] private Account? selectedCashAccount;
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private CashRegisterSummary? todaySummary;
    [ObservableProperty] private decimal actualClosingBalanceInput;
    [ObservableProperty] private string closingNotes = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var accounts = await _accountService.GetAllAsync(AccountType.Cash);
            CashAccounts.Clear();
            foreach (var a in accounts) CashAccounts.Add(a);

            SelectedCashAccount ??= CashAccounts.FirstOrDefault();

            if (SelectedCashAccount != null)
            {
                await RefreshDayAsync();
                await RefreshHistoryAsync();
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger la caisse.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshDayAsync()
    {
        if (SelectedCashAccount == null) return;
        TodaySummary = await _cashRegisterService.GetDaySummaryAsync(SelectedCashAccount.Id, SelectedDate);
        ActualClosingBalanceInput = TodaySummary.ActualClosingBalance ?? TodaySummary.ExpectedClosingBalance;
        ClosingNotes = TodaySummary.Notes ?? string.Empty;
    }

    private async Task RefreshHistoryAsync()
    {
        if (SelectedCashAccount == null) return;
        var history = await _cashRegisterService.GetHistoryAsync(SelectedCashAccount.Id, SelectedDate.AddDays(-13), SelectedDate);
        History.Clear();
        foreach (var day in history.OrderByDescending(d => d.Date)) History.Add(day);
    }

    partial void OnSelectedCashAccountChanged(Account? value) => _ = ReloadDayAndHistoryAsync();
    partial void OnSelectedDateChanged(DateTime value) => _ = ReloadDayAndHistoryAsync();

    private async Task ReloadDayAndHistoryAsync()
    {
        if (SelectedCashAccount == null) return;
        await RefreshDayAsync();
        await RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task CloseDayAsync()
    {
        if (SelectedCashAccount == null) return;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var username = _currentUserService.User?.FullName ?? "system";
            await _cashRegisterService.CloseDayAsync(SelectedCashAccount.Id, SelectedDate, ActualClosingBalanceInput, username, ClosingNotes);
            await RefreshDayAsync();
            await RefreshHistoryAsync();
            StatusMessage = "Caisse clôturée avec succès.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de clôturer la caisse.";
        }
    }

    [RelayCommand]
    private async Task ReopenDayAsync()
    {
        if (SelectedCashAccount == null) return;
        try
        {
            await _cashRegisterService.ReopenDayAsync(SelectedCashAccount.Id, SelectedDate);
            await RefreshDayAsync();
            await RefreshHistoryAsync();
            StatusMessage = "Caisse rouverte.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de rouvrir la caisse.";
        }
    }
}
