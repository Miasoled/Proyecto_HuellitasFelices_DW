using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        
        [Range(0, 99999)]
        public int StockActual { get; set; }
        
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        // FK
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        
        [Display(Name = "Sucursal")]
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }
    }
}
