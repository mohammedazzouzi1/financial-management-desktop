namespace MizanFinance.Core.Enums;

public enum UserRole
{
    Administrator,
    Manager,
    Accountant,
    Viewer
}

public enum AccountType
{
    Cash,
    Bank
}

public enum CurrencyCode
{
    MAD,
    EUR,
    USD,
    GBP
}

public enum CategoryType
{
    Revenue,
    Expense
}

public enum TransactionType
{
    Revenue,
    Expense,
    Transfer,
    Deposit,
    Withdrawal,
    Refund,
    PaymentReceived,
    PaymentIssued,
    Other
}

public enum PaymentMethod
{
    Cash,
    Cheque,
    BankTransfer,
    Card,
    Other
}

public enum PartyType
{
    None,
    Client,
    Supplier
}

public enum AuditAction
{
    Created,
    Modified,
    Deleted,
    Paid,
    Cancelled,
    Restored,
    LoggedIn,
    LoggedOut
}
