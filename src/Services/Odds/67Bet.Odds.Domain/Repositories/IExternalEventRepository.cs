using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Odds.Domain.Entities;

namespace _67Bet.Odds.Domain.Repositories;

public interface IExternalEventRepository
{
    Task<ExternalEvent?> GetByIdAsync(Guid id);
    Task<ExternalEvent?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<ExternalEvent>> GetAllActiveAsync();
    Task AddAsync(ExternalEvent @event);
    Task UpdateAsync(ExternalEvent @event);
}
