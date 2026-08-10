using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;

namespace MizanFinance.Data.Seed;

public static class DbInitializer
{
    public static async Task MigrateAsync(IDbContextFactory<MizanDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
        await SeedDefaultCategoriesAsync(db);
    }

    private static async Task SeedDefaultCategoriesAsync(MizanDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var expenseCategories = new[]
        {
            "Loyer", "Salaires", "Charges", "Transport", "Achats",
            "Marketing", "Maintenance", "Impôts et taxes", "Frais bancaires", "Frais de bureau", "Autre"
        };
        var revenueCategories = new[] { "Ventes", "Prestations de services", "Autres revenus" };

        foreach (var name in expenseCategories)
            db.Categories.Add(new Category { Name = name, Type = CategoryType.Expense, IsSystem = true });

        foreach (var name in revenueCategories)
            db.Categories.Add(new Category { Name = name, Type = CategoryType.Revenue, IsSystem = true });

        await db.SaveChangesAsync();
    }
}
