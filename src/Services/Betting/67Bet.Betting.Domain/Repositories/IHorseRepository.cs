using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Domain.Entities.VirtualRacing;

namespace _67Bet.Betting.Domain.Repositories
{
    public interface IHorseRepository
    {
        Task<IEnumerable<Horse>> GetAllAsync();
        Task<Horse?> GetByIdAsync(Guid id);
        Task AddAsync(Horse horse);
    }
}