namespace HuellitasFelices.Services;

public interface IContextProviderService
{
    Task<string> ObtenerContextoAsync(string preguntaUsuario);
}
