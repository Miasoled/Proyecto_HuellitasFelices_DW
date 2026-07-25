using System.Text;
using System.Text.Json;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Services;

public class OllamaService : IAIService
{
    private readonly HttpClient _http;
    private readonly AiSettings _settings;
    private readonly ILogger<IAIService> _logger;

    public OllamaService(HttpClient http, IOptions<AiSettings> settings, ILogger<IAIService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> GenerateAsync(string instruction, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                model = _settings.ModelName,
                prompt = instruction,
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{_settings.BaseUrl}/api/generate", content, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("response", out var responseProp))
            {
                return responseProp.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comunicarse con Ollama");
            return null;
        }
    }
}
