using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Entities;

public class CompanySettings : EntityBase
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public string? LogoPath { get; set; }
    public CurrencyCode DefaultCurrency { get; set; } = CurrencyCode.MAD;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public int FinancialYearStartMonth { get; set; } = 1;
    public string Language { get; set; } = "fr";
    public bool IsFirstRunComplete { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public int NextInvoiceNumber { get; set; } = 1;
}
