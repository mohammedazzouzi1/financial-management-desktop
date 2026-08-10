using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;

namespace MizanFinance.Core.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(CategoryType? type = null);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}

public interface IClientService
{
    Task<List<Client>> GetAllAsync(bool includeInactive = false);
    Task<Client?> GetByIdAsync(int id);
    Task<Client> CreateAsync(Client client);
    Task UpdateAsync(Client client);
    Task SetActiveAsync(int id, bool isActive);
    Task<decimal> GetBalanceAsync(int clientId);
}

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(bool includeInactive = false);
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> CreateAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task SetActiveAsync(int id, bool isActive);
    Task<decimal> GetBalanceAsync(int supplierId);
}

public interface ISettingsService
{
    Task<CompanySettings> GetSettingsAsync();
    Task UpdateSettingsAsync(CompanySettings settings);
}
