using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models;

public class Venta
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string? NumeroVenta { get; set; }

    public int ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }

    public int? DuenoId { get; set; }
    public Dueno? Dueno { get; set; }

    [Required]
    [Range(0.01, 99999)]
    public decimal TotalConsulta { get; set; }

    [Required]
    [Range(0, 99999)]
    public decimal TotalMedicamentos { get; set; }

    [Required]
    [Range(0.01, 99999)]
    public decimal Total => TotalConsulta + TotalMedicamentos;

    [Required]
    [MaxLength(30)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(30)]
    public string? MetodoPago { get; set; }

    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

    public DateTime? FechaPago { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime? FechaEliminacion { get; set; }

    [MaxLength(100)]
    public string? EliminadoPor { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
