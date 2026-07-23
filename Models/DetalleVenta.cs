using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Required]
    [Range(1, 9999)]
    public int Cantidad { get; set; }

    [Required]
    [Range(0.01, 99999)]
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;
}
