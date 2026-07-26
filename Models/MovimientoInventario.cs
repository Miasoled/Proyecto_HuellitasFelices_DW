using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TipoMovimiento { get; set; } = string.Empty;
        // Compra, Venta, Ajuste, Devolucion
        
        [Range(1, 99999)]
        public int Cantidad { get; set; }
        
        public int StockAnterior { get; set; }
        public int StockPosterior { get; set; }
        
        [StringLength(200)]
        public string? Referencia { get; set; }
        
        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? UsuarioId { get; set; }
        
        [StringLength(300)]
        public string? Observacion { get; set; }
        
        // FK
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        
        [Display(Name = "Compra")]
        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [Display(Name = "Sucursal destino")]
        public int? SucursalDestinoId { get; set; }
        public Sucursal? SucursalDestino { get; set; }
    }
}
