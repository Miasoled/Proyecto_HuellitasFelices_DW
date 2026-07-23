using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public interface IAuditService
    {
        Task LogAsync(string accion, string entidad, int? entidadId = null,
            string? usuarioId = null, string? usuarioEmail = null,
            string? direccionIP = null, string? valorAnterior = null,
            string? valorNuevo = null, string? descripcion = null,
            string nivel = "Info");
        
        Task<List<AuditLog>> GetLogsAsync(int pagina = 1, int tamanioPagina = 20,
            string? busqueda = null, string? accion = null, string? entidad = null);
        
        Task<int> GetTotalLogsAsync();
    }
}
