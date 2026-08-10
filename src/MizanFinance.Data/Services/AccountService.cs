using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Dto;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class AccountService : IAccountService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;

    public AccountService(IDbContextFactory<MizanDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Account>> GetAllAsync(AccountType? type = null, bool includeInactive = false)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Accounts.AsQueryable();
        if (type.HasValue) query = query.Where(a => a.Type == type.Value);
        if (!includeInactive) query = query.Where(a => a.IsActive);
        return await query.OrderBy(a => a.Name).ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        using var db = await _factory.CreateDbContextAsync();
        account.CurrentBalance = account.OpeningBalance;
        account.CreatedAt = DateTime.Now;
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Accounts.FindAsync(account.Id)
            ?? throw new InvalidOperationException("Account not found.");

        existing.Name = account.Name;
        existing.Type = account.Type;
        existing.BankName = account.BankName;
        existing.AccountNumber = account.AccountNumber;
        existing.Iban = account.Iban;
        existing.Currency = account.Currency;
        existing.OpeningBalance = account.OpeningBalance;
        existing.Notes = account.Notes;
        existing.ModifiedAt = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        using var db = await _factory.CreateDbContextAsync();
        var account = await db.Accounts.FindAsync(id);
        if (account == null) return;
        account.IsActive = isActive;
        account.ModifiedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<decimal> RecalculateBalanceAsync(int accountId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return 0m;

        var outgoing = await db.Transactions
            .Where(t => t.AccountId == accountId && t.PaymentMethod != PaymentMethod.Cheque)
            .ToListAsync();
        var incomingTransfers = await db.Transactions
            .Where(t => t.TransferToAccountId == accountId && t.Type == TransactionType.Transfer)
            .ToListAsync();

        decimal balance = account.OpeningBalance;
        foreach (var t in outgoing)
        {
            balance += t.Type switch
            {
                TransactionType.Revenue or TransactionType.Deposit or TransactionType.PaymentReceived or TransactionType.Refund => t.Amount,
                TransactionType.Expense or TransactionType.Withdrawal or TransactionType.PaymentIssued => -t.Amount,
                TransactionType.Transfer => -t.Amount,
                _ => 0m
            };
        }
        foreach (var t in incomingTransfers)
        {
            balance += t.Amount;
        }

        account.CurrentBalance = balance;
        await db.SaveChangesAsync();
        return balance;
    }

    public async Task<List<ChartPoint>> GetBalanceEvolutionAsync(int accountId, DateTime from, DateTime to)
    {
        using var db = await _factory.CreateDbContextAsync();
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return new List<ChartPoint>();

        var transactions = await db.Transactions
            .Where(t => (t.AccountId == accountId || t.TransferToAccountId == accountId)
                        && t.PaymentMethod != PaymentMethod.Cheque
                        && t.Date <= to)
            .OrderBy(t => t.Date)
            .ToListAsync();

        var beforeRange = transactions.Where(t => t.Date < from).ToList();
        decimal balanceAtStart = account.OpeningBalance;
        foreach (var t in beforeRange) balanceAtStart += SignedDelta(t, accountId);

        var points = new List<ChartPoint>();
        decimal running = balanceAtStart;
        var byDay = transactions.Where(t => t.Date >= from && t.Date <= to)
            .GroupBy(t => t.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(t => SignedDelta(t, accountId)));

        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            if (byDay.TryGetValue(day, out var delta)) running += delta;
            points.Add(new ChartPoint { Date = day, Label = day.ToString("dd/MM"), Value = running });
        }

        return points;
    }

    private static decimal SignedDelta(Transaction t, int accountId)
    {
        if (t.Type == TransactionType.Transfer)
        {
            if (t.AccountId == accountId) return -t.Amount;
            if (t.TransferToAccountId == accountId) return t.Amount;
            return 0m;
        }

        return t.Type switch
        {
            TransactionType.Revenue or TransactionType.Deposit or TransactionType.PaymentReceived or TransactionType.Refund => t.Amount,
            TransactionType.Expense or TransactionType.Withdrawal or TransactionType.PaymentIssued => -t.Amount,
            _ => 0m
        };
    }
}
