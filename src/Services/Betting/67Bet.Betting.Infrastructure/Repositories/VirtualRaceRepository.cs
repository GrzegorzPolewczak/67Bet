using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;

namespace _67Bet.Betting.Infrastructure.Repositories
{
    public class VirtualRaceRepository : IVirtualRaceRepository
    {
        private readonly BettingDbContext _context;

        public VirtualRaceRepository(BettingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VirtualRace>> GetActiveRacesAsync()
        {
            return await _context.VirtualRaces
                .Include(r => r.Participants)
                .ThenInclude(p => p.Horse)
                .Where(r => !r.IsFinished)
                .OrderBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<VirtualRace?> GetByIdAsync(Guid id)
        {
            return await _context.VirtualRaces
                .Include(r => r.Participants)
                .ThenInclude(p => p.Horse)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(VirtualRace race)
        {
            await _context.VirtualRaces.AddAsync(race);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VirtualRace race)
        {
            _context.VirtualRaces.Update(race);
            await _context.SaveChangesAsync();
        }
    }
}