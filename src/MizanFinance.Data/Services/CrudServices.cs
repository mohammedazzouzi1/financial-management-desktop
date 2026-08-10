using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using MizanFinance.Core.Interfaces;

namespace MizanFinance.Data.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;
    public CategoryService(IDbContextFactory<MizanDbContext> factory) => _factory = factory;

    public async Task<List<Category>> GetAllAsync(CategoryType? type = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Categories.AsQueryable();
        if (type.HasValue) query = query.Where(c => c.Type == type.Value);
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        using var db = await _factory.CreateDbContextAsync();
        category.ModifiedAt = DateTime.Now;
        db.Categories.Update(category);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var category = await db.Categories.FindAsync(id);
        if (category == null || category.IsSystem) return;
        category.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}

public class ClientService : IClientService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;
    public ClientService(IDbContextFactory<MizanDbContext> factory) => _factory = factory;

    public async Task<List<Client>> GetAllAsync(bool includeInactive = false)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Clients.AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Clients.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        using var db = await _factory.CreateDbContextAsync();
        client.ModifiedAt = DateTime.Now;
        db.Clients.Update(client);
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        using var db = await _factory.CreateDbContextAsync();
        var client = await db.Clients.FindAsync(id);
        if (client == null) return;
        client.IsActive = isActive;
        await db.SaveChangesAsync();
    }

    public async Task<decimal> GetBalanceAsync(int clientId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var tx = await db.Transactions.Where(t => t.ClientId == clientId).ToListAsync();
        return tx.Sum(t => t.IsIncome ? t.Amount : t.IsExpense ? -t.Amount : 0m);
    }
}

public class SupplierService : ISupplierService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;
    public SupplierService(IDbContextFactory<MizanDbContext> factory) => _factory = factory;

    public async Task<List<Supplier>> GetAllAsync(bool includeInactive = false)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Suppliers.AsQueryable();
        if (!includeInactive) query = query.Where(s => s.IsActive);
        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Supplier> CreateAsync(Supplier supplier)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        using var db = await _factory.CreateDbContextAsync();
        supplier.ModifiedAt = DateTime.Now;
        db.Suppliers.Update(supplier);
        await db.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        using var db = await _factory.CreateDbContextAsync();
        var supplier = await db.Suppliers.FindAsync(id);
        if (supplier == null) return;
        supplier.IsActive = isActive;
        await db.SaveChangesAsync();
    }

    public async Task<decimal> GetBalanceAsync(int supplierId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var tx = await db.Transactions.Where(t => t.SupplierId == supplierId).ToListAsync();
        return tx.Sum(t => t.IsExpense ? -t.Amount : t.IsIncome ? t.Amount : 0m);
    }
}

public class SettingsService : ISettingsService
{
    private readonly IDbContextFactory<MizanDbContext> _factory;
    public SettingsService(IDbContextFactory<MizanDbContext> factory) => _factory = factory;

    public async Task<CompanySettings> GetSettingsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        var settings = await db.CompanySettings.FirstOrDefaultAsync();
        if (settings != null) return settings;

        settings = new CompanySettings { CompanyName = "Mon Entreprise" };
        db.CompanySettings.Add(settings);
        await db.SaveChangesAsync();
        return settings;
    }

    public async Task UpdateSettingsAsync(CompanySettings settings)
    {
        using var db = await _factory.CreateDbContextAsync();
        settings.ModifiedAt = DateTime.Now;
        db.CompanySettings.Update(settings);
        await db.SaveChangesAsync();
    }
}
