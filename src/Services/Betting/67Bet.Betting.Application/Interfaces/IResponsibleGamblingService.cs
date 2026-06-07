using _67Bet.Betting.Application.DTOs;

namespace _67Bet.Betting.Application.Interfaces;

public interface IResponsibleGamblingClock
{
    DateTime UtcNow { get; }
}

public interface IResponsibleGamblingService
{
    Task<ResponsibleGamblingDashboardDto> GetDashboardAsync(Guid userId);
    Task<ResponsibleGamblingLimitDto> SetLimitAsync(Guid userId, SetResponsibleGamblingLimitRequest request);
    Task<SelfExclusionDto> StartSelfExclusionAsync(Guid userId, StartSelfExclusionRequest request);
    Task<ResponsibleGamblingValidationResultDto> ValidateStakeAsync(Guid userId, decimal amount);
    Task<ResponsibleGamblingValidationResultDto> ValidateDepositAsync(Guid userId, decimal amount);
    Task RecordActivityAsync(Guid userId, RecordResponsibleGamblingActivityRequest request);
}
