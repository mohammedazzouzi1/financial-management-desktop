using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class AccountEditViewModel : ObservableObject
{
    private readonly IAccountService _accountService;
    private readonly int? _editingId;

    public AccountEditViewModel(IAccountService accountService, Account? existing, AccountType defaultType)
    {
        _accountService = accountService;

        if (existing != null)
        {
            _editingId = existing.Id;
            Name = existing.Name;
            SelectedType = existing.Type;
            BankName = existing.BankName ?? string.Empty;
            AccountNumber = existing.AccountNumber ?? string.Empty;
            Iban = existing.Iban ?? string.Empty;
            SelectedCurrency = existing.Currency;
            OpeningBalance = existing.OpeningBalance;
            Notes = existing.Notes ?? string.Empty;
        }
        else
        {
            SelectedType = defaultType;
        }
    }

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Modifier le compte" : "Nouveau compte";
    public bool IsBankAccount => SelectedType == AccountType.Bank;

    public List<AccountType> AvailableTypes { get; } = Enum.GetValues<AccountType>().ToList();
    public List<CurrencyCode> AvailableCurrencies { get; } = Enum.GetValues<CurrencyCode>().ToList();

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private AccountType selectedType = AccountType.Bank;
    [ObservableProperty] private string bankName = string.Empty;
    [ObservableProperty] private string accountNumber = string.Empty;
    [ObservableProperty] private string iban = string.Empty;
    [ObservableProperty] private CurrencyCode selectedCurrency = CurrencyCode.MAD;
    [ObservableProperty] private decimal openingBalance;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    partial void OnSelectedTypeChanged(AccountType value) => OnPropertyChanged(nameof(IsBankAccount));

    public event Action? Saved;
    public event Action? Cancelled;

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Veuillez saisir un nom de compte.";
            return;
        }

        IsBusy = true;
        try
        {
            if (IsEditMode)
            {
                var account = new Account
                {
                    Id = _editingId!.Value,
                    Name = Name.Trim(),
                    Type = SelectedType,
                    BankName = IsBankAccount ? BankName : null,
                    AccountNumber = IsBankAccount ? AccountNumber : null,
                    Iban = IsBankAccount ? Iban : null,
                    Currency = SelectedCurrency,
                    OpeningBalance = OpeningBalance,
                    Notes = Notes,
                    IsActive = true
                };
                await _accountService.UpdateAsync(account);
            }
            else
            {
                await _accountService.CreateAsync(new Account
                {
                    Name = Name.Trim(),
                    Type = SelectedType,
                    BankName = IsBankAccount ? BankName : null,
                    AccountNumber = IsBankAccount ? AccountNumber : null,
                    Iban = IsBankAccount ? Iban : null,
                    Currency = SelectedCurrency,
                    OpeningBalance = OpeningBalance,
                    Notes = Notes,
                    IsActive = true
                });
            }

            Saved?.Invoke();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible d'enregistrer le compte.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
