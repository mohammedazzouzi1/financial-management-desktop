using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class Transaction : EntityBase
{
    public DateTime Date { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.MAD;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    // Used only for Transfer transactions: the destination account.
    public int? TransferToAccountId { get; set; }
    public Account? TransferToAccount { get; set; }

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public bool IsIncome => Type is TransactionType.Revenue or TransactionType.Deposit
        or TransactionType.PaymentReceived or TransactionType.Refund;

    public bool IsExpense => Type is TransactionType.Expense or TransactionType.Withdrawal
        or TransactionType.PaymentIssued;
}
