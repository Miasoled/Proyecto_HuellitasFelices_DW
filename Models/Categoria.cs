using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(300)]
        public string? Descripcion { get; set; }
        
        public bool Activo { get; set; } = true;
        
        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }
        
        [StringLength(100)]
        public string? EliminadoPor { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
