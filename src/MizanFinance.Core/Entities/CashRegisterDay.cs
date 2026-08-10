namespace MizanFinance.Core.Entities;

public class CashRegisterDay : EntityBase
{
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime Date { get; set; }
    public decimal OpeningBalance { get; set; }

    // Set once the register is closed for the day; null while still open.
    public decimal? ActualClosingBalance { get; set; }
    public string? Notes { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
}
