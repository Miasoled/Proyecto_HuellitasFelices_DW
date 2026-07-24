namespace HuellitasFelices.Services;

public interface IOllamaService
{
    Task<string?> GenerarRespuestaAsync(string prompt, CancellationToken ct = default);
}
