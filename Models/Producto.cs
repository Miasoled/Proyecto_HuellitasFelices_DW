using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Producto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(300)]
        public string? Descripcion { get; set; }
        
        [Required]
        [Range(0.01, 99999)]
        public decimal PrecioCompra { get; set; }
        
        [Required]
        [Range(0.01, 99999)]
        public decimal PrecioVenta { get; set; }
        
        [StringLength(50)]
        public string? CodigoBarras { get; set; }
        
        [StringLength(50)]
        public string? UnidadMedida { get; set; } = "Unidad";
        
        [Range(0, 99999)]
        public int StockMinimo { get; set; } = 5;
        
        public bool Activo { get; set; } = true;
        
        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }
        
        [StringLength(100)]
        public string? EliminadoPor { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        // FK
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        
        [Display(Name = "Proveedor")]
        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
        
        // Navegación
        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}
