using _67Bet.Odds.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _67Bet.Odds.Infrastructure.Persistence;

public class OddsDbContext : DbContext
{
    public DbSet<ExternalEvent> ExternalEvents { get; set; }
    public DbSet<ExternalMarket> ExternalMarkets { get; set; }
    public DbSet<ExternalOutcome> ExternalOutcomes { get; set; }

    public OddsDbContext(DbContextOptions<OddsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.ExternalId).IsUnique();
            builder.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<ExternalMarket>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.HasOne<ExternalEvent>()
                   .WithMany(e => e.Markets)
                   .HasForeignKey(m => m.ExternalEventId);
        });

        modelBuilder.Entity<ExternalOutcome>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Price).HasPrecision(10, 2);
            builder.HasOne<ExternalMarket>()
                   .WithMany(m => m.Outcomes)
                   .HasForeignKey(o => o.ExternalMarketId);
        });
    }
}
