using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Betting.Infrastructure.Persistence;
using _67Bet.Betting.Infrastructure.Repositories;
using _67Bet.Betting.Infrastructure.Integrations;

namespace _67Bet.Betting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BettingDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IMarketRepository, MarketRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAiMatchInsightRepository, AiMatchInsightRepository>();

        services.AddHttpClient<IOddsServiceClient, OddsServiceClient>();
        services.AddHttpClient<IWalletService, WalletServiceClient>();

        services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        });
        services.AddScoped<IVirtualRaceRepository, VirtualRaceRepository>();
        services.AddScoped<IHorseRepository, HorseRepository>();

        // Gamification Repositories
        services.AddScoped<IUserGamificationRepository, UserGamificationRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();

        // Responsible Gambling Repositories
        services.AddScoped<IResponsibleGamblingLimitRepository, ResponsibleGamblingLimitRepository>();
        services.AddScoped<ISelfExclusionRepository, SelfExclusionRepository>();
        services.AddScoped<IResponsibleGamblingActivityRepository, ResponsibleGamblingActivityRepository>();

        return services;
    }
}
