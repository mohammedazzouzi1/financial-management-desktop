namespace MizanFinance.Core.Dto;

public class CashRegisterSummary
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public decimal ExpectedClosingBalance => OpeningBalance + CashIn - CashOut;
    public decimal? ActualClosingBalance { get; set; }
    public decimal? Discrepancy => ActualClosingBalance.HasValue ? ActualClosingBalance.Value - ExpectedClosingBalance : null;
    public bool IsClosed { get; set; }
    public string? Notes { get; set; }
}
