using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _67Bet.Identity.Domain.Repositories;
using _67Bet.Identity.Infrastructure.Persistence;
using _67Bet.Identity.Infrastructure.Repositories;

namespace _67Bet.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseInMemoryDatabase("IdentityDb"));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
