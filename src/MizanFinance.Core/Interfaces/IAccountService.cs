using MizanFinance.Core.Dto;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Interfaces;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync(AccountType? type = null, bool includeInactive = false);
    Task<Account?> GetByIdAsync(int id);
    Task<Account> CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task SetActiveAsync(int id, bool isActive);
    Task<decimal> RecalculateBalanceAsync(int accountId);
    Task<List<ChartPoint>> GetBalanceEvolutionAsync(int accountId, DateTime from, DateTime to);
}
