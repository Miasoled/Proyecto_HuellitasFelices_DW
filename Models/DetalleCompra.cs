using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }
        
        [Range(1, 99999)]
        public int Cantidad { get; set; }
        
        [Required]
        [Range(0.01, 99999)]
        public decimal PrecioUnitario { get; set; }
        
        public decimal Subtotal => Cantidad * PrecioUnitario;
        
        // FK
        [Display(Name = "Compra")]
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }
        
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}
