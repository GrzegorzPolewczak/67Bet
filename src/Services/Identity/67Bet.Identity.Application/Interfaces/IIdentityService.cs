using System;
using System.Threading.Tasks;
using _67Bet.Identity.Domain.Entities;

namespace _67Bet.Identity.Application.Interfaces;

/*
 * Interfejs IIdentityService obsługuje procesy związane z tożsamością użytkowników.
 * Obejmuje rejestrację, logowanie (autentykację) oraz pobieranie informacji o profilu.
 */
public interface IIdentityService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> RegisterAsync(string username, string email, string password);
    Task<bool> AuthenticateAsync(string email, string password);
}
