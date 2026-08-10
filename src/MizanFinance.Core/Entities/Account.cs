using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class Account : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; } = AccountType.Cash;
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? Iban { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.MAD;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
