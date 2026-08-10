using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Dto;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class CashRegisterService : ICashRegisterService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;

    public CashRegisterService(IDbContextFactory<MizanDbContext> factory)
    {
        _factory = factory;
    }

    private static async Task<decimal> GetOpeningBalanceAsync(MizanDbContext db, int accountId, DateTime date)
    {
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return 0m;

        var priorDay = await db.CashRegisterDays
            .Where(c => c.AccountId == accountId && c.Date < date.Date && c.IsClosed)
            .OrderByDescending(c => c.Date)
            .FirstOrDefaultAsync();

        if (priorDay?.ActualClosingBalance != null)
            return priorDay.ActualClosingBalance.Value;

        // No prior closed day: derive opening balance from account opening balance plus all movements before this date.
        var priorTransactions = await db.Transactions
            .Where(t => t.AccountId == accountId && t.PaymentMethod != PaymentMethod.Cheque && t.Date < date.Date)
            .ToListAsync();

        decimal balance = account.OpeningBalance;
        foreach (var t in priorTransactions)
        {
            balance += t.Type switch
            {
                TransactionType.Revenue or TransactionType.Deposit or TransactionType.PaymentReceived or TransactionType.Refund => t.Amount,
                TransactionType.Expense or TransactionType.Withdrawal or TransactionType.PaymentIssued or TransactionType.Transfer => -t.Amount,
                _ => 0m
            };
        }
        return balance;
    }

    public async Task<CashRegisterSummary> GetDaySummaryAsync(int cashAccountId, DateTime date)
    {
        using var db = await _factory.CreateDbContextAsync();
        var account = await db.Accounts.FindAsync(cashAccountId);
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var movements = await db.Transactions
            .Where(t => t.AccountId == cashAccountId && t.PaymentMethod != PaymentMethod.Cheque
                        && t.Date >= dayStart && t.Date <= dayEnd)
            .ToListAsync();

        var cashIn = movements.Where(t => t.IsIncome).Sum(t => t.Amount);
        var cashOut = movements.Where(t => t.IsExpense || t.Type == TransactionType.Transfer).Sum(t => t.Amount);

        var openingBalance = await GetOpeningBalanceAsync(db, cashAccountId, date);

        var registerDay = await db.CashRegisterDays
            .FirstOrDefaultAsync(c => c.AccountId == cashAccountId && c.Date == dayStart);

        return new CashRegisterSummary
        {
            AccountId = cashAccountId,
            AccountName = account?.Name ?? string.Empty,
            Date = dayStart,
            OpeningBalance = openingBalance,
            CashIn = cashIn,
            CashOut = cashOut,
            ActualClosingBalance = registerDay?.ActualClosingBalance,
            IsClosed = registerDay?.IsClosed ?? false,
            Notes = registerDay?.Notes
        };
    }

    public async Task<List<CashRegisterSummary>> GetHistoryAsync(int cashAccountId, DateTime from, DateTime to)
    {
        var results = new List<CashRegisterSummary>();
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            results.Add(await GetDaySummaryAsync(cashAccountId, day));
        }
        return results;
    }

    public async Task CloseDayAsync(int cashAccountId, DateTime date, decimal actualClosingBalance, string closedBy, string? notes)
    {
        using var db = await _factory.CreateDbContextAsync();
        var dayStart = date.Date;
        var registerDay = await db.CashRegisterDays
            .FirstOrDefaultAsync(c => c.AccountId == cashAccountId && c.Date == dayStart);

        var openingBalance = await GetOpeningBalanceAsync(db, cashAccountId, date);

        if (registerDay == null)
        {
            registerDay = new CashRegisterDay
            {
                AccountId = cashAccountId,
                Date = dayStart,
                OpeningBalance = openingBalance,
                CreatedAt = DateTime.Now
            };
            db.CashRegisterDays.Add(registerDay);
        }

        registerDay.OpeningBalance = openingBalance;
        registerDay.ActualClosingBalance = actualClosingBalance;
        registerDay.IsClosed = true;
        registerDay.ClosedAt = DateTime.Now;
        registerDay.ClosedBy = closedBy;
        registerDay.Notes = notes;
        registerDay.ModifiedAt = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task ReopenDayAsync(int cashAccountId, DateTime date)
    {
        using var db = await _factory.CreateDbContextAsync();
        var dayStart = date.Date;
        var registerDay = await db.CashRegisterDays
            .FirstOrDefaultAsync(c => c.AccountId == cashAccountId && c.Date == dayStart);
        if (registerDay == null) return;

        registerDay.IsClosed = false;
        registerDay.ModifiedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }
}
