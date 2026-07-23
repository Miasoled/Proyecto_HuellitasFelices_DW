using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? UsuarioId { get; set; }
        
        [StringLength(100)]
        public string? UsuarioEmail { get; set; }
        
        [StringLength(50)]
        public string? DireccionIP { get; set; }
        
        [StringLength(50)]
        public string Accion { get; set; } = string.Empty; // Login, Logout, Create, Update, Delete, PaymentApproved, etc.
        
        [StringLength(100)]
        public string Entidad { get; set; } = string.Empty; // Consulta, Pago, Mascota, etc.
        
        public int? IdentificadorEntidad { get; set; }
        
        [StringLength(2000)]
        public string? ValorAnterior { get; set; }
        
        [StringLength(2000)]
        public string? ValorNuevo { get; set; }
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
        
        [StringLength(50)]
        public string? Nivel { get; set; } = "Info"; // Info, Warning, Error, Critical
    }
}
