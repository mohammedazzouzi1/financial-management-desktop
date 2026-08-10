using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Interfaces;

public class TransactionFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? AccountId { get; set; }
    public TransactionType? Type { get; set; }
    public int? CategoryId { get; set; }
    public int? ClientId { get; set; }
    public int? SupplierId { get; set; }
    public string? SearchText { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 100;
}

public interface ITransactionService
{
    Task<(List<Transaction> Items, int TotalCount)> GetAsync(TransactionFilter filter);
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction> CreateAsync(Transaction transaction, string username);
    Task UpdateAsync(Transaction transaction, string username);
    Task DeleteAsync(int id, string username);
}
