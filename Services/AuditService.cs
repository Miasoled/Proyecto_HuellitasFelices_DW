using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        
        public AuditService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task LogAsync(string accion, string entidad, int? entidadId = null,
            string? usuarioId = null, string? usuarioEmail = null,
            string? direccionIP = null, string? valorAnterior = null,
            string? valorNuevo = null, string? descripcion = null,
            string nivel = "Info")
        {
            var log = new AuditLog
            {
                Accion = accion,
                Entidad = entidad,
                IdentificadorEntidad = entidadId,
                UsuarioId = usuarioId,
                UsuarioEmail = usuarioEmail,
                DireccionIP = direccionIP,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                Descripcion = descripcion,
                Nivel = nivel,
                FechaCreacion = DateTime.UtcNow
            };
            
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<AuditLog>> GetLogsAsync(int pagina = 1, int tamanioPagina = 20,
            string? busqueda = null, string? accion = null, string? entidad = null)
        {
            var query = _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.FechaCreacion)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(l =>
                    (l.UsuarioEmail != null && l.UsuarioEmail.Contains(busqueda)) ||
                    (l.Descripcion != null && l.Descripcion.Contains(busqueda)) ||
                    (l.DireccionIP != null && l.DireccionIP.Contains(busqueda)));
            
            if (!string.IsNullOrEmpty(accion))
                query = query.Where(l => l.Accion == accion);
            
            if (!string.IsNullOrEmpty(entidad))
                query = query.Where(l => l.Entidad == entidad);
            
            return await query
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();
        }
        
        public async Task<int> GetTotalLogsAsync()
            => await _context.AuditLogs.CountAsync();
    }
}
