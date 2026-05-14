using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _67Bet.Wallet.Domain.Repositories;
using _67Bet.Wallet.Infrastructure.Persistence;
using _67Bet.Wallet.Infrastructure.Repositories;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Infrastructure.Services;

namespace _67Bet.Wallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<WalletDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentService, StripePaymentService>();

        return services;
    }
}
