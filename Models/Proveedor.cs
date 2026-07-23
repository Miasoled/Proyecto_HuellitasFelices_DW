using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }
        
        [StringLength(200)]
        public string? Direccion { get; set; }
        
        [StringLength(20)]
        public string? RUC { get; set; }
        
        public bool Activo { get; set; } = true;
        
        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }
        
        [StringLength(100)]
        public string? EliminadoPor { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}
