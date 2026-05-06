using System;
using System.Threading.Tasks;
using _67Bet.Identity.Application.Interfaces;
using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;

namespace _67Bet.Identity.Application.Services;

/*
 * Serwis IdentityService implementuje logikę zarządzania użytkownikami.
 * Odpowiada za bezpieczną rejestrację, weryfikację poświadczeń oraz zarządzanie danymi konta.
 */
public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;

    public IdentityService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null) throw new InvalidOperationException("Użytkownik o tym adresie e-mail już istnieje.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password); 

        var user = new User(username, email, passwordHash);
        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<bool> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}
