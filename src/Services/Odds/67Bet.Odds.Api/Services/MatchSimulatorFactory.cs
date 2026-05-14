using System;
using System.Collections.Generic;
using System.Linq;

namespace _67Bet.Odds.Api.Services;

public class MatchSimulatorFactory
{
    private readonly IEnumerable<ILiveMatchSimulator> _simulators;

    public MatchSimulatorFactory(IEnumerable<ILiveMatchSimulator> simulators)
    {
        _simulators = simulators;
    }

    public ILiveMatchSimulator GetSimulator(string sportKey)
    {
        var simulator = _simulators.FirstOrDefault(s => sportKey.Contains(s.SportKey, StringComparison.OrdinalIgnoreCase));
        return simulator ?? _simulators.First(s => s.SportKey == "default");
    }
}
