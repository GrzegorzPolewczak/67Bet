namespace _67Bet.Betting.Domain.Enums;

public enum ResponsibleLimitType
{
    SingleStake = 1,
    DailyStake = 2,
    WeeklyLoss = 3,
    DailyDeposit = 4
}

public enum ResponsibleLimitPeriod
{
    None = 0,
    Daily = 1,
    Weekly = 2
}

public enum ResponsibleGamblingActivityType
{
    Stake = 1,
    Deposit = 2,
    Payout = 3
}
