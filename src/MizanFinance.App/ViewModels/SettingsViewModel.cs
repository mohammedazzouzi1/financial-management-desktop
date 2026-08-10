using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [ObservableProperty] private CompanySettings? settings;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public List<CurrencyCode> AvailableCurrencies { get; } = Enum.GetValues<CurrencyCode>().ToList();

    [RelayCommand]
    private async Task LoadAsync()
    {
        Settings = await _settingsService.GetSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Settings == null) return;
        StatusMessage = string.Empty;
        ErrorMessage = string.Empty;
        try
        {
            await _settingsService.UpdateSettingsAsync(Settings);
            StatusMessage = "Paramètres enregistrés.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible d'enregistrer les paramètres.";
        }
    }
}
