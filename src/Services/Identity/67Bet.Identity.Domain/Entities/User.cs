/*
 * Plik zawiera encję User oraz interfejs IUserRepository.
 * Definiuje model użytkownika systemu oraz metody dostępu do danych tożsamościowych.
 */
using System;
using _67Bet.Shared.Kernel;
using System.Threading.Tasks;

namespace _67Bet.Identity.Domain.Enums
{
    public enum Role
    {
        Admin,
        User
    }
}

namespace _67Bet.Identity.Domain.Entities
{
    using _67Bet.Identity.Domain.Enums;

    public class User : BaseEntity, IAggregateRoot
    {
        public string Username { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public Role UserRole { get; private set; }

        public User(string username, string email, string passwordHash, Role role = Role.User)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            UserRole = role;
        }

        public void ChangeRole(Role newRole) => UserRole = newRole;

        // EF Core
        private User() { }
    }
}

namespace _67Bet.Identity.Domain.Repositories
{
    using _67Bet.Identity.Domain.Entities;

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}

