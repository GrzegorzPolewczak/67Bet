using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities.Roulette;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;

namespace _67Bet.Betting.Infrastructure.Repositories;

public sealed class EfRouletteRoundRepository : IRouletteRoundRepository
{
    private readonly BettingDbContext _context;

    public EfRouletteRoundRepository(BettingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RouletteRound round)
    {
        await _context.RouletteRounds.AddAsync(round);
        await _context.SaveChangesAsync();
    }

    public async Task<RouletteRound?> GetByIdAsync(Guid roundId)
    {
        return await _context.RouletteRounds
            .Include(r => r.Bets)
            .FirstOrDefaultAsync(r => r.Id == roundId);
    }

    public async Task UpdateAsync(RouletteRound round)
    {
        _context.RouletteRounds.Update(round);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<RouletteRound>> GetRecentForUserAsync(Guid userId, int limit)
    {
        return await _context.RouletteRounds
            .Include(r => r.Bets)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArrayAsync();
    }
}
