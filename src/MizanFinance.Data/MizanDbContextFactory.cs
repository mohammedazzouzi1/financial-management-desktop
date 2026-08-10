using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MizanFinance.Data;

public class MizanDbContextFactory : IDesignTimeDbContextFactory<MizanDbContext>
{
    public MizanDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MizanDbContext>()
            .UseSqlite("Data Source=mizanfinance.design.db")
            .Options;
        return new MizanDbContext(options);
    }
}
