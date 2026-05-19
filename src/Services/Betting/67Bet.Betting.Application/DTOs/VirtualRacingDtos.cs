using System;
using System.Collections.Generic;

namespace _67Bet.Betting.Application.DTOs
{
    public record VirtualRaceDto(
        Guid Id,
        string Name,
        DateTime StartTime,
        bool IsFinished,
        Guid? WinningHorseId,
        IEnumerable<VirtualRaceParticipantDto> Participants
    );

    public record VirtualRaceParticipantDto(
        Guid Id,
        Guid HorseId,
        string HorseName,
        decimal Odds
    );

    public record HorseDto(
        Guid Id,
        string Name,
        int SkillLevel
    );
}