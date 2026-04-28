using System.Threading.Tasks;
using _67Bet.Odds.Application.Services;
using FluentAssertions;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class OddsServiceTests
{
    private readonly OddsService _oddsService;

    public OddsServiceTests()
    {
        _oddsService = new OddsService();
    }

    [Theory]
    [InlineData(0.5, 1.9)]   // (1/0.5) * 0.95 = 2.0 * 0.95 = 1.9
    [InlineData(0.25, 3.8)]  // (1/0.25) * 0.95 = 4.0 * 0.95 = 3.8
    [InlineData(0.8, 1.19)]  // (1/0.8) * 0.95 = 1.25 * 0.95 = 1.1875 -> 1.19
    public async Task CalculateOddsAsync_ShouldReturnCorrectOddsWithMargin(decimal probability, decimal expectedOdds)
    {
        // Act
        var result = await _oddsService.CalculateOddsAsync(probability);

        // Assert
        result.Should().Be(expectedOdds);
    }

    [Fact]
    public async Task CalculateOddsAsync_ShouldReturnMinimumOdds_WhenProbabilityIsOne()
    {
        // Act
        var result = await _oddsService.CalculateOddsAsync(1.0m);

        // Assert
        result.Should().Be(1.01m);
    }

    [Fact]
    public async Task CalculateOddsAsync_ShouldReturnHighOdds_WhenProbabilityIsZero()
    {
        // Act
        var result = await _oddsService.CalculateOddsAsync(0m);

        // Assert
        result.Should().Be(100.0m);
    }
}
