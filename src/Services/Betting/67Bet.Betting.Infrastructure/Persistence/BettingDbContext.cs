/*
 * Kontekst bazy danych dla modułu Betting.
 * Definiuje mapowania ORM dla zdarzeń, rynków, wyników i kuponów.
 */
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities;

namespace _67Bet.Betting.Infrastructure.Persistence;

public class BettingDbContext : DbContext
{
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Outcome> Outcomes => Set<Outcome>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Bet> Bets => Set<Bet>();

    public BettingDbContext(DbContextOptions<BettingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sport>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Event>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Metadata).HasColumnType("jsonb");
            builder.HasOne<Sport>().WithMany().HasForeignKey(e => e.SportId);
        });

        modelBuilder.Entity<Market>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
            builder.HasMany(m => m.Outcomes).WithOne().HasForeignKey(o => o.MarketId);
        });

        modelBuilder.Entity<Outcome>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Probability).HasPrecision(5, 4);
            builder.Property(o => o.CurrentPrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.TotalOdds).HasPrecision(10, 2);
            builder.Property(t => t.Stake).HasPrecision(18, 2);
            builder.Property(t => t.PotentialWinning).HasPrecision(18, 2);
            builder.HasMany(t => t.Bets).WithOne().HasForeignKey(b => b.TicketId);
        });

        modelBuilder.Entity<Bet>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.FixedPrice).HasPrecision(10, 2);
        });
    }
}

