namespace MizanFinance.Core.Dto;

public class DashboardSummary
{
    public decimal TodayRevenue { get; set; }
    public decimal TodayExpenses { get; set; }
    public decimal TodayNetCashFlow => TodayRevenue - TodayExpenses;

    public decimal CurrentCashBalance { get; set; }
    public decimal TotalBankBalance { get; set; }
    public decimal TotalBalance => CurrentCashBalance + TotalBankBalance;

    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyProfit => MonthlyRevenue - MonthlyExpenses;

    public decimal TotalReceivables { get; set; }
    public decimal TotalPayables { get; set; }
}

public class ChartPoint
{
    public string Label { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal SecondaryValue { get; set; }
}

public enum DateRangePreset
{
    Today,
    Yesterday,
    ThisWeek,
    ThisMonth,
    LastMonth,
    ThisYear,
    LastYear,
    Custom
}

public class DateRangeFilter
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateRangePreset Preset { get; set; } = DateRangePreset.ThisMonth;

    public static DateRangeFilter FromPreset(DateRangePreset preset)
    {
        var today = DateTime.Today;
        return preset switch
        {
            DateRangePreset.Today => new DateRangeFilter { From = today, To = today, Preset = preset },
            DateRangePreset.Yesterday => new DateRangeFilter { From = today.AddDays(-1), To = today.AddDays(-1), Preset = preset },
            DateRangePreset.ThisWeek => new DateRangeFilter { From = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)), To = today, Preset = preset },
            DateRangePreset.ThisMonth => new DateRangeFilter { From = new DateTime(today.Year, today.Month, 1), To = today, Preset = preset },
            DateRangePreset.LastMonth => new DateRangeFilter
            {
                From = new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                To = new DateTime(today.Year, today.Month, 1).AddDays(-1),
                Preset = preset
            },
            DateRangePreset.ThisYear => new DateRangeFilter { From = new DateTime(today.Year, 1, 1), To = today, Preset = preset },
            DateRangePreset.LastYear => new DateRangeFilter { From = new DateTime(today.Year - 1, 1, 1), To = new DateTime(today.Year - 1, 12, 31), Preset = preset },
            _ => new DateRangeFilter { From = new DateTime(today.Year, today.Month, 1), To = today, Preset = preset }
        };
    }
}
