namespace HuellitasFelices.Settings;

public class AiSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "qwen2.5:0.5b";
    public int TimeoutSeconds { get; set; } = 30;
}
