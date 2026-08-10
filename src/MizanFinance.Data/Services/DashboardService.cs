using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Dto;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;

    public DashboardService(IDbContextFactory<MizanDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<DashboardSummary> GetSummaryAsync(DateRangeFilter filter)
    {
        using var db = await _factory.CreateDbContextAsync();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var accounts = await db.Accounts.Where(a => a.IsActive).ToListAsync();
        var cashBalance = accounts.Where(a => a.Type == AccountType.Cash).Sum(a => a.CurrentBalance);
        var bankBalance = accounts.Where(a => a.Type == AccountType.Bank).Sum(a => a.CurrentBalance);

        var todayTx = await db.Transactions
            .Where(t => t.Date >= today && t.Date < today.AddDays(1) && t.PaymentMethod != PaymentMethod.Cheque)
            .ToListAsync();

        var monthTx = await db.Transactions
            .Where(t => t.Date >= monthStart && t.Date < monthStart.AddMonths(1) && t.PaymentMethod != PaymentMethod.Cheque)
            .ToListAsync();

        return new DashboardSummary
        {
            TodayRevenue = todayTx.Where(t => t.IsIncome).Sum(t => t.Amount),
            TodayExpenses = todayTx.Where(t => t.IsExpense).Sum(t => t.Amount),
            CurrentCashBalance = cashBalance,
            TotalBankBalance = bankBalance,
            MonthlyRevenue = monthTx.Where(t => t.IsIncome).Sum(t => t.Amount),
            MonthlyExpenses = monthTx.Where(t => t.IsExpense).Sum(t => t.Amount),
            TotalReceivables = 0m,
            TotalPayables = 0m
        };
    }

    public async Task<List<ChartPoint>> GetRevenueVsExpenseAsync(DateRangeFilter filter)
    {
        using var db = await _factory.CreateDbContextAsync();
        var transactions = await db.Transactions
            .Where(t => t.Date >= filter.From.Date && t.Date <= filter.To.Date.AddDays(1).AddTicks(-1)
                        && t.PaymentMethod != PaymentMethod.Cheque)
            .ToListAsync();

        var points = new List<ChartPoint>();
        var groupByMonth = (filter.To - filter.From).TotalDays > 62;

        if (groupByMonth)
        {
            var groups = transactions
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key);
            foreach (var g in groups)
            {
                points.Add(new ChartPoint
                {
                    Date = g.Key,
                    Label = g.Key.ToString("MMM yyyy"),
                    Value = g.Where(t => t.IsIncome).Sum(t => t.Amount),
                    SecondaryValue = g.Where(t => t.IsExpense).Sum(t => t.Amount)
                });
            }
        }
        else
        {
            for (var day = filter.From.Date; day <= filter.To.Date; day = day.AddDays(1))
            {
                var dayTx = transactions.Where(t => t.Date.Date == day).ToList();
                points.Add(new ChartPoint
                {
                    Date = day,
                    Label = day.ToString("dd/MM"),
                    Value = dayTx.Where(t => t.IsIncome).Sum(t => t.Amount),
                    SecondaryValue = dayTx.Where(t => t.IsExpense).Sum(t => t.Amount)
                });
            }
        }

        return points;
    }

    public async Task<List<ChartPoint>> GetCashFlowEvolutionAsync(DateRangeFilter filter)
    {
        using var db = await _factory.CreateDbContextAsync();
        var accounts = await db.Accounts.ToListAsync();
        var openingTotal = accounts.Sum(a => a.OpeningBalance);

        var priorTx = await db.Transactions
            .Where(t => t.Date < filter.From.Date && t.PaymentMethod != PaymentMethod.Cheque && t.Type != TransactionType.Transfer)
            .ToListAsync();
        var runningStart = openingTotal + priorTx.Sum(t => t.IsIncome ? t.Amount : t.IsExpense ? -t.Amount : 0m);

        var rangeTx = await db.Transactions
            .Where(t => t.Date >= filter.From.Date && t.Date <= filter.To.Date.AddDays(1).AddTicks(-1)
                        && t.PaymentMethod != PaymentMethod.Cheque && t.Type != TransactionType.Transfer)
            .ToListAsync();

        var byDay = rangeTx.GroupBy(t => t.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.IsIncome ? t.Amount : t.IsExpense ? -t.Amount : 0m));

        var points = new List<ChartPoint>();
        var running = runningStart;
        for (var day = filter.From.Date; day <= filter.To.Date; day = day.AddDays(1))
        {
            if (byDay.TryGetValue(day, out var delta)) running += delta;
            points.Add(new ChartPoint { Date = day, Label = day.ToString("dd/MM"), Value = running });
        }

        return points;
    }
}
