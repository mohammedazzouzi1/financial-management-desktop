using MizanFinance.Core.Dto;

namespace MizanFinance.Core.Interfaces;

public interface ICashRegisterService
{
    Task<CashRegisterSummary> GetDaySummaryAsync(int cashAccountId, DateTime date);
    Task<List<CashRegisterSummary>> GetHistoryAsync(int cashAccountId, DateTime from, DateTime to);
    Task CloseDayAsync(int cashAccountId, DateTime date, decimal actualClosingBalance, string closedBy, string? notes);
    Task ReopenDayAsync(int cashAccountId, DateTime date);
}
