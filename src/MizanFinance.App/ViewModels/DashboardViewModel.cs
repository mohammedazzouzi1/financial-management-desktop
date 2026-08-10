using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MizanFinance.Core.Dto;
using MizanFinance.Core.Interfaces;
using SkiaSharp;

namespace MizanFinance.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        RevenueExpenseSeries = new ISeries[]
        {
            new ColumnSeries<decimal>
            {
                Name = "Revenus",
                Values = Array.Empty<decimal>(),
                Fill = new SolidColorPaint(new SKColor(0x16, 0xA3, 0x4A)),
                MaxBarWidth = 18
            },
            new ColumnSeries<decimal>
            {
                Name = "Dépenses",
                Values = Array.Empty<decimal>(),
                Fill = new SolidColorPaint(new SKColor(0xDC, 0x26, 0x26)),
                MaxBarWidth = 18
            }
        };

        CashFlowSeries = new ISeries[]
        {
            new LineSeries<decimal>
            {
                Name = "Trésorerie",
                Values = Array.Empty<decimal>(),
                Fill = null,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(0x25, 0x63, 0xEB), 3)
            }
        };

        RevenueExpenseXAxes = new Axis[] { new() { Labels = new List<string>(), LabelsRotation = 0, TextSize = 11 } };
        CashFlowXAxes = new Axis[] { new() { Labels = new List<string>(), LabelsRotation = 0, TextSize = 11 } };
    }

    [ObservableProperty] private DashboardSummary summary = new();
    [ObservableProperty] private DateRangePreset selectedPreset = DateRangePreset.ThisMonth;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ISeries[] RevenueExpenseSeries { get; private set; }
    public ISeries[] CashFlowSeries { get; private set; }
    public Axis[] RevenueExpenseXAxes { get; private set; }
    public Axis[] CashFlowXAxes { get; private set; }

    public List<DateRangePreset> AvailablePresets { get; } = Enum.GetValues<DateRangePreset>()
        .Where(p => p != DateRangePreset.Custom).ToList();

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var filter = DateRangeFilter.FromPreset(SelectedPreset);

            Summary = await _dashboardService.GetSummaryAsync(filter);

            var revenueExpensePoints = await _dashboardService.GetRevenueVsExpenseAsync(filter);
            ((ColumnSeries<decimal>)RevenueExpenseSeries[0]).Values = revenueExpensePoints.Select(p => p.Value).ToArray();
            ((ColumnSeries<decimal>)RevenueExpenseSeries[1]).Values = revenueExpensePoints.Select(p => p.SecondaryValue).ToArray();
            RevenueExpenseXAxes[0].Labels = revenueExpensePoints.Select(p => p.Label).ToList();

            var cashFlowPoints = await _dashboardService.GetCashFlowEvolutionAsync(filter);
            ((LineSeries<decimal>)CashFlowSeries[0]).Values = cashFlowPoints.Select(p => p.Value).ToArray();
            CashFlowXAxes[0].Labels = cashFlowPoints.Select(p => p.Label).ToList();

            OnPropertyChanged(nameof(RevenueExpenseSeries));
            OnPropertyChanged(nameof(CashFlowSeries));
            OnPropertyChanged(nameof(RevenueExpenseXAxes));
            OnPropertyChanged(nameof(CashFlowXAxes));
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger le tableau de bord. Veuillez réessayer.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedPresetChanged(DateRangePreset value)
    {
        _ = LoadAsync();
    }
}
