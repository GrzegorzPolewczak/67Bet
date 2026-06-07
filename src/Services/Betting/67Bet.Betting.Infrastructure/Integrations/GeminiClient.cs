using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using _67Bet.Betting.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Infrastructure.Integrations;

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key missing.");
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        try
        {
            var url = $"v1/models/gemini-3.1-flash-lite:generateContent?key={_apiKey}";
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                   ?? "Nie udało się wygenerować podpowiedzi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed.");
            throw;
        }
    }

    // Modele pomocnicze dla JSONa Gemini
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
