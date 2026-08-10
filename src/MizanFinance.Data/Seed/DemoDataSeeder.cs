using Microsoft.EntityFrameworkCore;
using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;

namespace MizanFinance.Data.Seed;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IDbContextFactory<MizanDbContext> factory, string createdBy)
    {
        using var db = await factory.CreateDbContextAsync();
        if (await db.Clients.AnyAsync() || await db.Transactions.AnyAsync()) return;

        var cashAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Type == AccountType.Cash);
        var bankAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Type == AccountType.Bank);

        if (bankAccount == null)
        {
            bankAccount = new Account
            {
                Name = "Compte Principal",
                Type = AccountType.Bank,
                BankName = "Attijariwafa Bank",
                AccountNumber = "007780000123456789",
                Iban = "MA64007780000123456789012",
                Currency = CurrencyCode.MAD,
                OpeningBalance = 50000m,
                CurrentBalance = 50000m,
                IsActive = true
            };
            db.Accounts.Add(bankAccount);
        }

        if (cashAccount == null)
        {
            cashAccount = new Account
            {
                Name = "Caisse Principale",
                Type = AccountType.Cash,
                Currency = CurrencyCode.MAD,
                OpeningBalance = 5000m,
                CurrentBalance = 5000m,
                IsActive = true
            };
            db.Accounts.Add(cashAccount);
        }

        await db.SaveChangesAsync();

        var clients = new[]
        {
            new Client { Name = "Karim Benjelloun", Company = "Benjelloun Trading", Phone = "0661234567", Email = "karim@benjelloun.ma", IsActive = true },
            new Client { Name = "Société Atlas Textile", Company = "Atlas Textile SARL", Phone = "0522334455", Email = "contact@atlastextile.ma", IsActive = true },
            new Client { Name = "Fatima Zahra Idrissi", Company = "FZ Consulting", Phone = "0677889900", Email = "fz@consulting.ma", IsActive = true }
        };
        db.Clients.AddRange(clients);

        var suppliers = new[]
        {
            new Supplier { Name = "Office Depot Maroc", Company = "Office Depot", Phone = "0522112233", Email = "ventes@officedepot.ma", IsActive = true },
            new Supplier { Name = "Electro Fournitures", Company = "ElectroPlus", Phone = "0522998877", Email = "contact@electroplus.ma", IsActive = true },
            new Supplier { Name = "Imprimerie Al Andalous", Company = "Al Andalous", Phone = "0522445566", Email = "info@alandalous.ma", IsActive = true }
        };
        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync();

        var revenueCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Revenue && c.Name == "Ventes");
        var servicesCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Revenue && c.Name == "Prestations de services");
        var rentCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Expense && c.Name == "Loyer");
        var salariesCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Expense && c.Name == "Salaires");
        var suppliesCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Expense && c.Name == "Achats");
        var transportCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Expense && c.Name == "Transport");
        var marketingCategory = await db.Categories.FirstAsync(c => c.Type == CategoryType.Expense && c.Name == "Marketing");

        var random = new Random(42);
        var transactions = new List<Transaction>();
        var today = DateTime.Today;

        for (var i = 90; i >= 0; i--)
        {
            var date = today.AddDays(-i);

            if (date.Day == 1)
            {
                transactions.Add(new Transaction
                {
                    Date = date, Type = TransactionType.Expense, CategoryId = rentCategory.Id,
                    Amount = 8000m, PaymentMethod = PaymentMethod.BankTransfer, AccountId = bankAccount.Id,
                    Description = "Loyer mensuel du local", CreatedBy = createdBy
                });
                transactions.Add(new Transaction
                {
                    Date = date, Type = TransactionType.Expense, CategoryId = salariesCategory.Id,
                    Amount = 22000m, PaymentMethod = PaymentMethod.BankTransfer, AccountId = bankAccount.Id,
                    Description = "Salaires du personnel", CreatedBy = createdBy
                });
            }

            if (random.NextDouble() < 0.35)
            {
                var client = clients[random.Next(clients.Length)];
                var isCash = random.NextDouble() < 0.4;
                transactions.Add(new Transaction
                {
                    Date = date,
                    Type = TransactionType.Revenue,
                    CategoryId = random.NextDouble() < 0.6 ? revenueCategory.Id : servicesCategory.Id,
                    Amount = Math.Round((decimal)(random.Next(800, 15000)) / 10m * 10m, 2),
                    PaymentMethod = isCash ? PaymentMethod.Cash : PaymentMethod.BankTransfer,
                    AccountId = isCash ? cashAccount.Id : bankAccount.Id,
                    ClientId = client.Id,
                    Description = "Vente / prestation",
                    CreatedBy = createdBy
                });
            }

            if (random.NextDouble() < 0.25)
            {
                var supplier = suppliers[random.Next(suppliers.Length)];
                var categoryId = random.NextDouble() switch
                {
                    < 0.4 => suppliesCategory.Id,
                    < 0.7 => transportCategory.Id,
                    _ => marketingCategory.Id
                };
                var isCash = random.NextDouble() < 0.3;
                transactions.Add(new Transaction
                {
                    Date = date,
                    Type = TransactionType.Expense,
                    CategoryId = categoryId,
                    Amount = Math.Round((decimal)(random.Next(200, 6000)) / 10m * 10m, 2),
                    PaymentMethod = isCash ? PaymentMethod.Cash : PaymentMethod.BankTransfer,
                    AccountId = isCash ? cashAccount.Id : bankAccount.Id,
                    SupplierId = supplier.Id,
                    Description = "Achat / dépense fournisseur",
                    CreatedBy = createdBy
                });
            }
        }

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        decimal cashDelta = transactions.Where(t => t.AccountId == cashAccount.Id)
            .Sum(t => t.IsIncome ? t.Amount : t.IsExpense ? -t.Amount : 0m);
        decimal bankDelta = transactions.Where(t => t.AccountId == bankAccount.Id)
            .Sum(t => t.IsIncome ? t.Amount : t.IsExpense ? -t.Amount : 0m);

        cashAccount.CurrentBalance = cashAccount.OpeningBalance + cashDelta;
        bankAccount.CurrentBalance = bankAccount.OpeningBalance + bankDelta;
        await db.SaveChangesAsync();
    }

    public static async Task<bool> HasDemoDataAsync(IDbContextFactory<MizanDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        return await db.Clients.AnyAsync() || await db.Transactions.AnyAsync();
    }

    public static async Task RemoveDemoDataAsync(IDbContextFactory<MizanDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        db.Transactions.RemoveRange(db.Transactions);
        db.Clients.RemoveRange(db.Clients);
        db.Suppliers.RemoveRange(db.Suppliers);
        await db.SaveChangesAsync();

        foreach (var account in await db.Accounts.ToListAsync())
        {
            account.CurrentBalance = account.OpeningBalance;
        }
        await db.SaveChangesAsync();
    }
}
