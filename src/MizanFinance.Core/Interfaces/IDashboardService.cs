using MizanFinance.Core.Dto;

namespace MizanFinance.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(DateRangeFilter filter);
    Task<List<ChartPoint>> GetRevenueVsExpenseAsync(DateRangeFilter filter);
    Task<List<ChartPoint>> GetCashFlowEvolutionAsync(DateRangeFilter filter);
}
