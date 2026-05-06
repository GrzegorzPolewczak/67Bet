using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _67Bet.CustomBet.Domain.Repositories;
using _67Bet.CustomBet.Infrastructure.Persistence;
using _67Bet.CustomBet.Infrastructure.Repositories;

namespace _67Bet.CustomBet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CustomBetDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ICustomBetRepository, CustomBetRepository>();

        return services;
    }
}
