using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;

namespace _67Bet.Betting.Infrastructure.Repositories
{
    public class HorseRepository : IHorseRepository
    {
        private readonly BettingDbContext _context;

        public HorseRepository(BettingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Horse>> GetAllAsync()
        {
            return await _context.Horses.ToListAsync();
        }

        public async Task<Horse?> GetByIdAsync(Guid id)
        {
            return await _context.Horses.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task AddAsync(Horse horse)
        {
            await _context.Horses.AddAsync(horse);
            await _context.SaveChangesAsync();
        }
    }
}