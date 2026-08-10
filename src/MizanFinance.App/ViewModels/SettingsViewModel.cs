using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;
using MizanFinance.Data;
using MizanFinance.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace MizanFinance.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IDbContextFactory<MizanDbContext> _dbContextFactory;

    public SettingsViewModel(ISettingsService settingsService, IDbContextFactory<MizanDbContext> dbContextFactory)
    {
        _settingsService = settingsService;
        _dbContextFactory = dbContextFactory;
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

    [RelayCommand]
    private async Task RemoveDemoDataAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        ErrorMessage = string.Empty;
        try
        {
            await DemoDataSeeder.RemoveDemoDataAsync(_dbContextFactory);
            StatusMessage = "Données de démonstration supprimées.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de supprimer les données de démonstration.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
