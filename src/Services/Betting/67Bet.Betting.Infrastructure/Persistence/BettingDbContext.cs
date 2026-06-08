/*
 * Kontekst bazy danych dla modułu Betting.
 * Definiuje mapowania ORM dla zdarzeń, rynków, wyników i kuponów.
 */
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Entities.Roulette;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Entities.Gamification;
using _67Bet.Betting.Domain.Entities.ResponsibleGambling;

namespace _67Bet.Betting.Infrastructure.Persistence;

public class BettingDbContext : DbContext
{
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Outcome> Outcomes => Set<Outcome>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Bet> Bets => Set<Bet>();
    public DbSet<AiMatchInsight> AiMatchInsights => Set<AiMatchInsight>();

    // Virtual Racing
    public DbSet<VirtualRace> VirtualRaces => Set<VirtualRace>();
    public DbSet<Horse> Horses => Set<Horse>();
    public DbSet<VirtualRaceParticipant> VirtualRaceParticipants => Set<VirtualRaceParticipant>();

    // Gamification
    public DbSet<UserGamification> UserGamifications => Set<UserGamification>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    // Responsible Gambling
    public DbSet<ResponsibleGamblingLimit> ResponsibleGamblingLimits => Set<ResponsibleGamblingLimit>();
    public DbSet<SelfExclusion> SelfExclusions => Set<SelfExclusion>();
    public DbSet<ResponsibleGamblingActivity> ResponsibleGamblingActivities => Set<ResponsibleGamblingActivity>();

    // Roulette
    public DbSet<RouletteRound> RouletteRounds => Set<RouletteRound>();
    public DbSet<RouletteBet> RouletteBets => Set<RouletteBet>();

    public BettingDbContext(DbContextOptions<BettingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiMatchInsight>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.EventId).IsRequired();
            builder.Property(a => a.Content).IsRequired().HasColumnType("longtext");
            builder.HasIndex(a => a.EventId).IsUnique();
        });

        modelBuilder.Entity<AiGenerationLog>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.EventId).IsRequired();
            builder.Property(l => l.Status).IsRequired().HasMaxLength(50);
            builder.Property(l => l.ErrorMessage).HasColumnType("longtext");
        });

        modelBuilder.Entity<Sport>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Event>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
            builder.Property(e => e.Metadata).HasColumnType("longtext");
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
            builder.Property(o => o.Probability).HasColumnType("decimal(18,4)");
            builder.Property(o => o.CurrentPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.TotalOdds).HasColumnType("decimal(18,2)");
            builder.Property(t => t.Stake).HasColumnType("decimal(18,2)");
            builder.Property(t => t.PotentialWinning).HasColumnType("decimal(18,2)");
            builder.HasMany(t => t.Bets).WithOne().HasForeignKey(b => b.TicketId);
        });

        modelBuilder.Entity<Bet>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.OutcomeName).IsRequired().HasMaxLength(255);
            builder.Property(b => b.MarketName).IsRequired().HasMaxLength(100);
            builder.Property(b => b.EventName).IsRequired().HasMaxLength(255);
            builder.Property(b => b.WinningOutcomeName).HasMaxLength(255);
            builder.Property(b => b.StartTime).HasColumnType("datetime(6)");
            builder.Property(b => b.FixedPrice).HasColumnType("decimal(18,2)");
        });

        // Virtual Racing Configurations
        modelBuilder.Entity<VirtualRace>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
            builder.HasMany(r => r.Participants)
                   .WithOne(p => p.Race)
                   .HasForeignKey(p => p.RaceId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Horse>(builder =>
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<VirtualRaceParticipant>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Odds).HasColumnType("decimal(18,2)");
            builder.HasOne(p => p.Horse)
                   .WithMany()
                   .HasForeignKey(p => p.HorseId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        // Gamification Configurations
        modelBuilder.Entity<UserGamification>(builder =>
        {
            builder.HasKey(ug => ug.Id);
            builder.HasIndex(ug => ug.UserId).IsUnique();
        });

        modelBuilder.Entity<Achievement>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Threshold).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<UserAchievement>(builder =>
        {
            builder.HasKey(ua => ua.Id);
            builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();
            builder.Property(ua => ua.CurrentProgress).HasColumnType("decimal(18,2)");
        });

        // Responsible Gambling Configurations
        modelBuilder.Entity<ResponsibleGamblingLimit>(builder =>
        {
            builder.HasKey(limit => limit.Id);
            builder.HasIndex(limit => new { limit.UserId, limit.Type }).IsUnique();
            builder.Property(limit => limit.Amount).HasColumnType("decimal(18,2)");
            builder.Property(limit => limit.PendingAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<SelfExclusion>(builder =>
        {
            builder.HasKey(exclusion => exclusion.Id);
            builder.HasIndex(exclusion => exclusion.UserId);
            builder.Property(exclusion => exclusion.Reason).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<ResponsibleGamblingActivity>(builder =>
        {
            builder.HasKey(activity => activity.Id);
            builder.HasIndex(activity => new { activity.UserId, activity.OccurredAtUtc });
            builder.Property(activity => activity.Amount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<RouletteRound>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.UserId);
            builder.Property(r => r.TotalStake).HasColumnType("decimal(18,2)");
            builder.Property(r => r.TotalPayout).HasColumnType("decimal(18,2)");
            builder.Property(r => r.CreatedAtUtc).HasColumnType("datetime(6)");
            builder.HasMany(r => r.Bets)
                   .WithOne()
                   .HasForeignKey("RouletteRoundId")
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(r => r.Bets).HasField("_bets");
        });

        modelBuilder.Entity<RouletteBet>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.BetType).HasConversion<int>();
            builder.Property(b => b.Stake).HasColumnType("decimal(18,2)");
            builder.Property(b => b.Payout).HasColumnType("decimal(18,2)");
        });
    }
}

