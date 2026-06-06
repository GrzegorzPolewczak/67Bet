using Microsoft.Extensions.DependencyInjection;
using _67Bet.CustomBet.Application.Interfaces;
using _67Bet.CustomBet.Application.Services;

namespace _67Bet.CustomBet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomBetService, CustomBetService>();

        return services;
    }
}
