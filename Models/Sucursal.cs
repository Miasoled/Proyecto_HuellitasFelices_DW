using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Sucursal
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Direccion { get; set; }
        
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        public bool Activo { get; set; } = true;
        
        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }
        
        [StringLength(100)]
        public string? EliminadoPor { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
        public ICollection<MovimientoInventario> MovimientosOrigen { get; set; } = new List<MovimientoInventario>();
        public ICollection<MovimientoInventario> MovimientosDestino { get; set; } = new List<MovimientoInventario>();
    }
}
