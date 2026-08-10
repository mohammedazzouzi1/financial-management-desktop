using MizanFinance.Core.Entities;
using MizanFinance.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace MizanFinance.Data;

public class MizanDbContext : DbContext
{
    public MizanDbContext(DbContextOptions<MizanDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CashRegisterDay> CashRegisterDays => Set<CashRegisterDay>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>();
            e.Property(x => x.Currency).HasConversion<string>();
            e.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            e.Property(x => x.CurrentBalance).HasPrecision(18, 2);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>();
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Client>(e =>
        {
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>();
            e.Property(x => x.Currency).HasConversion<string>();
            e.Property(x => x.PaymentMethod).HasConversion<string>();
            e.Property(x => x.Amount).HasPrecision(18, 2);

            e.HasOne(x => x.Account).WithMany(a => a.Transactions).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TransferToAccount).WithMany().HasForeignKey(x => x.TransferToAccountId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Client).WithMany(c => c.Transactions).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Supplier).WithMany(s => s.Transactions).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.Date);
            e.HasIndex(x => x.AccountId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<CashRegisterDay>(e =>
        {
            e.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            e.Property(x => x.ActualClosingBalance).HasPrecision(18, 2);
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.AccountId, x.Date }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(x => x.Action).HasConversion<string>();
            e.HasIndex(x => x.Timestamp);
        });

        modelBuilder.Entity<CompanySettings>(e =>
        {
            e.Property(x => x.DefaultCurrency).HasConversion<string>();
        });
    }
}
