namespace HuellitasFelices.Services;

public interface IAIService
{
    Task<string?> GenerateAsync(string instruction, CancellationToken ct = default);
}
