using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;

    public TransactionService(IDbContextFactory<MizanDbContext> factory)
    {
        _factory = factory;
    }

    // Cheques do not move cash until the Cheque module clears/deposits them (see project spec section 36).
    private static decimal ComputeSingleAccountDelta(Transaction t)
    {
        if (t.PaymentMethod == PaymentMethod.Cheque)
            return 0m;

        return t.Type switch
        {
            TransactionType.Revenue => t.Amount,
            TransactionType.Deposit => t.Amount,
            TransactionType.PaymentReceived => t.Amount,
            TransactionType.Refund => t.Amount,
            TransactionType.Expense => -t.Amount,
            TransactionType.Withdrawal => -t.Amount,
            TransactionType.PaymentIssued => -t.Amount,
            _ => 0m
        };
    }

    private static async Task ApplyBalanceEffectAsync(MizanDbContext db, Transaction t, int sign)
    {
        if (t.Type == TransactionType.Transfer && t.TransferToAccountId.HasValue)
        {
            var from = await db.Accounts.FindAsync(t.AccountId);
            var to = await db.Accounts.FindAsync(t.TransferToAccountId.Value);
            if (from != null) from.CurrentBalance -= sign * t.Amount;
            if (to != null) to.CurrentBalance += sign * t.Amount;
        }
        else
        {
            var delta = ComputeSingleAccountDelta(t);
            if (delta == 0m) return;
            var account = await db.Accounts.FindAsync(t.AccountId);
            if (account != null) account.CurrentBalance += sign * delta;
        }
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetAsync(TransactionFilter filter)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Client)
            .Include(t => t.Supplier)
            .AsQueryable();

        if (filter.From.HasValue) query = query.Where(t => t.Date >= filter.From.Value);
        if (filter.To.HasValue) query = query.Where(t => t.Date <= filter.To.Value.Date.AddDays(1).AddTicks(-1));
        if (filter.AccountId.HasValue) query = query.Where(t => t.AccountId == filter.AccountId.Value || t.TransferToAccountId == filter.AccountId.Value);
        if (filter.Type.HasValue) query = query.Where(t => t.Type == filter.Type.Value);
        if (filter.CategoryId.HasValue) query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (filter.ClientId.HasValue) query = query.Where(t => t.ClientId == filter.ClientId.Value);
        if (filter.SupplierId.HasValue) query = query.Where(t => t.SupplierId == filter.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var s = filter.SearchText.Trim();
            query = query.Where(t =>
                (t.Description != null && t.Description.Contains(s)) ||
                (t.Reference != null && t.Reference.Contains(s)) ||
                (t.InvoiceNumber != null && t.InvoiceNumber.Contains(s)) ||
                (t.ChequeNumber != null && t.ChequeNumber.Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Client)
            .Include(t => t.Supplier)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, string username)
    {
        using var db = await _factory.CreateDbContextAsync();
        transaction.CreatedBy = username;
        transaction.CreatedAt = DateTime.Now;
        db.Transactions.Add(transaction);
        await ApplyBalanceEffectAsync(db, transaction, sign: 1);
        await db.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, string username)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == transaction.Id)
            ?? throw new InvalidOperationException("Transaction not found.");

        await ApplyBalanceEffectAsync(db, existing, sign: -1);

        transaction.ModifiedAt = DateTime.Now;
        db.Transactions.Update(transaction);

        await ApplyBalanceEffectAsync(db, transaction, sign: 1);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string username)
    {
        using var db = await _factory.CreateDbContextAsync();
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return;

        await ApplyBalanceEffectAsync(db, transaction, sign: -1);
        transaction.IsDeleted = true;
        transaction.ModifiedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }
}
