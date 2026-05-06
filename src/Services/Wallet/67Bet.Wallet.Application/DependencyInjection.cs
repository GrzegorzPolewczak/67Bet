using Microsoft.Extensions.DependencyInjection;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.Services;

namespace _67Bet.Wallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, WalletService>();
        
        return services;
    }
}
