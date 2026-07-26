using System.Diagnostics;
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
        var sw = Stopwatch.StartNew();
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

            _logger.LogInformation("IA request iniciado -> modelo={Model}, timeout={Timeout}s",
                _settings.ModelName, _settings.TimeoutSeconds);

            var response = await _http.PostAsync($"{_settings.BaseUrl}/api/generate", content, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("response", out var responseProp))
            {
                sw.Stop();
                _logger.LogInformation("IA response OK en {Elapsed}ms, largo={Length}",
                    sw.ElapsedMilliseconds, responseProp.GetString()?.Length ?? 0);
                return responseProp.GetString();
            }

            sw.Stop();
            _logger.LogWarning("IA response sin campo 'response' en {Elapsed}ms", sw.ElapsedMilliseconds);
            return null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            sw.Stop();
            _logger.LogWarning("IA request cancelado por el usuario en {Elapsed}ms", sw.ElapsedMilliseconds);
            return null;
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            _logger.LogError("IA request timeout después de {Elapsed}ms (timeout={Timeout}s)",
                sw.ElapsedMilliseconds, _settings.TimeoutSeconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            _logger.LogError(ex, "IA request falló (HTTP {StatusCode}) en {Elapsed}ms",
                ex.StatusCode, sw.ElapsedMilliseconds);
            return null;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error inesperado en IA en {Elapsed}ms", sw.ElapsedMilliseconds);
            return null;
        }
    }
}
