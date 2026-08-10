using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class FirstRunViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IAuthService _authService;
    private readonly IAccountService _accountService;

    public FirstRunViewModel(
        ISettingsService settingsService,
        IAuthService authService,
        IAccountService accountService)
    {
        _settingsService = settingsService;
        _authService = authService;
        _accountService = accountService;
    }

    public List<CurrencyCode> AvailableCurrencies { get; } = Enum.GetValues<CurrencyCode>().ToList();

    public static readonly string[] StepTitles =
    {
        "Entreprise", "Devise", "Administrateur", "Caisse", "Banque", "Catégories", "Terminer"
    };

    [ObservableProperty] private int currentStepIndex;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public bool IsFirstStep => CurrentStepIndex == 0;
    public bool IsLastStep => CurrentStepIndex == StepTitles.Length - 1;
    public string StepIndicatorText => $"Étape {CurrentStepIndex + 1} sur {StepTitles.Length} — {StepTitles[CurrentStepIndex]}";

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(StepIndicatorText));
    }

    // Step 1: Company info
    [ObservableProperty] private string companyName = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string taxId = string.Empty;

    // Step 2: Currency
    [ObservableProperty] private CurrencyCode selectedCurrency = CurrencyCode.MAD;

    // Step 3: Administrator account
    [ObservableProperty] private string adminUsername = "admin";
    [ObservableProperty] private string adminFullName = string.Empty;
    public Func<string>? AdminPasswordAccessor { get; set; }
    public Func<string>? AdminPasswordConfirmAccessor { get; set; }

    // Step 4: First cash account
    [ObservableProperty] private string cashAccountName = "Caisse Principale";
    [ObservableProperty] private decimal cashOpeningBalance;

    // Step 5: Bank account (optional)
    [ObservableProperty] private bool includeBankAccount;
    [ObservableProperty] private string bankAccountName = "Compte Principal";
    [ObservableProperty] private string bankName = string.Empty;
    [ObservableProperty] private string bankAccountNumber = string.Empty;
    [ObservableProperty] private string bankIban = string.Empty;
    [ObservableProperty] private decimal bankOpeningBalance;

    // Step 6: Categories (informational — defaults already seeded)
    // Step 7: Finish

    public bool IsFinished { get; private set; }

    public event Action? SetupCompleted;
    public event Action? SetupCancelled;

    [RelayCommand]
    private void Next()
    {
        ErrorMessage = string.Empty;

        if (CurrentStepIndex == 0 && string.IsNullOrWhiteSpace(CompanyName))
        {
            ErrorMessage = "Veuillez saisir le nom de votre entreprise.";
            return;
        }

        if (CurrentStepIndex == 2)
        {
            if (string.IsNullOrWhiteSpace(AdminUsername) || string.IsNullOrWhiteSpace(AdminFullName))
            {
                ErrorMessage = "Veuillez renseigner le nom d'utilisateur et le nom complet.";
                return;
            }

            var pwd = AdminPasswordAccessor?.Invoke() ?? string.Empty;
            var pwdConfirm = AdminPasswordConfirmAccessor?.Invoke() ?? string.Empty;
            if (pwd.Length < 4)
            {
                ErrorMessage = "Le mot de passe doit contenir au moins 4 caractères.";
                return;
            }
            if (pwd != pwdConfirm)
            {
                ErrorMessage = "Les mots de passe ne correspondent pas.";
                return;
            }
        }

        if (CurrentStepIndex == 3 && string.IsNullOrWhiteSpace(CashAccountName))
        {
            ErrorMessage = "Veuillez nommer votre compte de caisse.";
            return;
        }

        if (CurrentStepIndex < 6) CurrentStepIndex++;
    }

    [RelayCommand]
    private void Back()
    {
        ErrorMessage = string.Empty;
        if (CurrentStepIndex > 0) CurrentStepIndex--;
    }

    [RelayCommand]
    private void Cancel() => SetupCancelled?.Invoke();

    [RelayCommand]
    private async Task FinishAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            settings.CompanyName = CompanyName.Trim();
            settings.Address = Address;
            settings.Phone = Phone;
            settings.Email = Email;
            settings.TaxId = TaxId;
            settings.DefaultCurrency = SelectedCurrency;
            settings.IsFirstRunComplete = true;
            await _settingsService.UpdateSettingsAsync(settings);

            var password = AdminPasswordAccessor?.Invoke() ?? "admin";
            await _authService.CreateUserAsync(
                AdminUsername.Trim(), password, AdminFullName.Trim(), UserRole.Administrator, Email);

            await _accountService.CreateAsync(new Account
            {
                Name = CashAccountName.Trim(),
                Type = AccountType.Cash,
                Currency = SelectedCurrency,
                OpeningBalance = CashOpeningBalance,
                IsActive = true
            });

            if (IncludeBankAccount && !string.IsNullOrWhiteSpace(BankAccountName))
            {
                await _accountService.CreateAsync(new Account
                {
                    Name = BankAccountName.Trim(),
                    Type = AccountType.Bank,
                    BankName = BankName,
                    AccountNumber = BankAccountNumber,
                    Iban = BankIban,
                    Currency = SelectedCurrency,
                    OpeningBalance = BankOpeningBalance,
                    IsActive = true
                });
            }

            IsFinished = true;
            SetupCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible de terminer la configuration initiale : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
