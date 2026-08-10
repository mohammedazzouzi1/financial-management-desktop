using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.App.Services;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class TransactionEditViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ICategoryService _categoryService;
    private readonly IClientService _clientService;
    private readonly ISupplierService _supplierService;
    private readonly CurrentUserService _currentUserService;

    private readonly int? _editingId;

    public TransactionEditViewModel(
        ITransactionService transactionService,
        IAccountService accountService,
        ICategoryService categoryService,
        IClientService clientService,
        ISupplierService supplierService,
        CurrentUserService currentUserService,
        Transaction? existing)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
        _clientService = clientService;
        _supplierService = supplierService;
        _currentUserService = currentUserService;

        if (existing != null)
        {
            _editingId = existing.Id;
            Date = existing.Date;
            SelectedType = existing.Type;
            SelectedPaymentMethod = existing.PaymentMethod;
            Amount = existing.Amount;
            AccountId = existing.AccountId;
            TransferToAccountId = existing.TransferToAccountId;
            CategoryId = existing.CategoryId;
            ClientId = existing.ClientId;
            SupplierId = existing.SupplierId;
            Description = existing.Description ?? string.Empty;
            Reference = existing.Reference ?? string.Empty;
            InvoiceNumber = existing.InvoiceNumber ?? string.Empty;
            ChequeNumber = existing.ChequeNumber ?? string.Empty;
            Notes = existing.Notes ?? string.Empty;
        }
    }

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Modifier la transaction" : "Nouvelle transaction";

    public List<TransactionType> AvailableTypes { get; } = Enum.GetValues<TransactionType>().ToList();
    public List<PaymentMethod> AvailablePaymentMethods { get; } = Enum.GetValues<PaymentMethod>().ToList();

    public ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private TransactionType selectedType = TransactionType.Revenue;
    [ObservableProperty] private PaymentMethod selectedPaymentMethod = PaymentMethod.Cash;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private int accountId;
    [ObservableProperty] private int? transferToAccountId;
    [ObservableProperty] private int? categoryId;
    [ObservableProperty] private int? clientId;
    [ObservableProperty] private int? supplierId;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string reference = string.Empty;
    [ObservableProperty] private string invoiceNumber = string.Empty;
    [ObservableProperty] private string chequeNumber = string.Empty;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public bool IsTransfer => SelectedType == TransactionType.Transfer;
    public bool ShowChequeField => SelectedPaymentMethod == PaymentMethod.Cheque;

    partial void OnSelectedTypeChanged(TransactionType value)
    {
        OnPropertyChanged(nameof(IsTransfer));
    }

    partial void OnSelectedPaymentMethodChanged(PaymentMethod value)
    {
        OnPropertyChanged(nameof(ShowChequeField));
    }

    public event Action? Saved;
    public event Action? Cancelled;

    [RelayCommand]
    private async Task LoadLookupsAsync()
    {
        var accounts = await _accountService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();
        var clients = await _clientService.GetAllAsync();
        var suppliers = await _supplierService.GetAllAsync();

        Accounts.Clear();
        foreach (var a in accounts) Accounts.Add(a);
        Categories.Clear();
        foreach (var c in categories) Categories.Add(c);
        Clients.Clear();
        foreach (var c in clients) Clients.Add(c);
        Suppliers.Clear();
        foreach (var s in suppliers) Suppliers.Add(s);

        if (AccountId == 0 && Accounts.Count > 0) AccountId = Accounts[0].Id;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (Amount <= 0)
        {
            ErrorMessage = "Le montant doit être supérieur à zéro.";
            return;
        }
        if (AccountId == 0)
        {
            ErrorMessage = "Veuillez sélectionner un compte.";
            return;
        }
        if (IsTransfer && (!TransferToAccountId.HasValue || TransferToAccountId.Value == AccountId))
        {
            ErrorMessage = "Veuillez sélectionner un compte de destination différent du compte source.";
            return;
        }

        IsBusy = true;
        try
        {
            var username = _currentUserService.User?.Username ?? "system";
            var transaction = new Transaction
            {
                Id = _editingId ?? 0,
                Date = Date,
                Type = SelectedType,
                PaymentMethod = SelectedPaymentMethod,
                Amount = Amount,
                AccountId = AccountId,
                TransferToAccountId = IsTransfer ? TransferToAccountId : null,
                CategoryId = CategoryId,
                ClientId = ClientId,
                SupplierId = SupplierId,
                Description = Description,
                Reference = Reference,
                InvoiceNumber = InvoiceNumber,
                ChequeNumber = ShowChequeField ? ChequeNumber : null,
                Notes = Notes
            };

            if (IsEditMode)
                await _transactionService.UpdateAsync(transaction, username);
            else
                await _transactionService.CreateAsync(transaction, username);

            Saved?.Invoke();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible d'enregistrer la transaction. Vérifiez les informations saisies.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
