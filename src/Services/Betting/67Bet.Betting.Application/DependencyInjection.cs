using Microsoft.Extensions.DependencyInjection;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;

namespace _67Bet.Betting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBettingService, BettingService>();
        services.AddScoped<IAiAssistantService, AiAssistantService>();
        services.AddScoped<IVirtualRacingService, VirtualRacingService>();
        services.AddScoped<IGamificationService, GamificationService>();
        return services;
    }
}
