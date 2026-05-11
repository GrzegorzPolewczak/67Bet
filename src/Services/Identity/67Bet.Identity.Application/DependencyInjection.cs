using Microsoft.Extensions.DependencyInjection;
using _67Bet.Identity.Application.Interfaces;
using _67Bet.Identity.Application.Services;

namespace _67Bet.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        
        return services;
    }
}
