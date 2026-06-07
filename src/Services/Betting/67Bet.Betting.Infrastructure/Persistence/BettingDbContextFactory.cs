using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace _67Bet.Betting.Infrastructure.Persistence;

public sealed class BettingDbContextFactory : IDesignTimeDbContextFactory<BettingDbContext>
{
    public BettingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string for BettingDbContext was not found. " +
                "Set ConnectionStrings__DefaultConnection before running dotnet ef.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<BettingDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));

        return new BettingDbContext(optionsBuilder.Options);
    }
}
