using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Application.Services;
using _67Bet.Odds.Domain.Repositories;
using _67Bet.Odds.Infrastructure.Integrations;
using _67Bet.Odds.Infrastructure.Persistence;
using _67Bet.Odds.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace _67Bet.Odds.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<OddsDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

        services.AddScoped<IExternalEventRepository, ExternalEventRepository>();

        services.AddHttpClient<ITheOddsApiClient, TheOddsApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.the-odds-api.com/");
        });

        services.AddHttpClient<IPandaScoreApiClient, PandaScoreApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.pandascore.co/");
        });

        services.AddHttpClient<ILiveDataProvider, ApiSportsClient>();

        services.AddScoped<IOddsIntegrationService, OddsIntegrationService>();

        return services;
    }
}
