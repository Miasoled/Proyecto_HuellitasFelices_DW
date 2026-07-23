using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models;

public class ConsultaMedicamento
{
    public int Id { get; set; }

    public int ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Required]
    [Range(1, 9999)]
    public int Cantidad { get; set; } = 1;

    [Required]
    [Range(0.01, 99999)]
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;

    [MaxLength(300)]
    public string? Dosis { get; set; }

    [MaxLength(300)]
    public string? Indicaciones { get; set; }
}
