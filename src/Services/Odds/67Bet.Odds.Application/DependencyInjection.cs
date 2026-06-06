using Microsoft.Extensions.DependencyInjection;
using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Application.Services;

namespace _67Bet.Odds.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOddsService, OddsService>();

        return services;
    }
}
