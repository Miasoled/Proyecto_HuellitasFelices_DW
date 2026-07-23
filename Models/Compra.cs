using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Compra
    {
        public int Id { get; set; }
        
        [StringLength(50)]
        public string NumeroCompra { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, 99999)]
        public decimal Total { get; set; }
        
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Recibida, Cancelada
        
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
        
        [StringLength(300)]
        public string? Observacion { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        // FK
        [Display(Name = "Proveedor")]
        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
        
        // Navegación
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}
