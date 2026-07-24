using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuellitasFelices.Models;

[Table("Pagos")]
public class Pago
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string NumeroPago { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 99999)]
    public decimal Monto { get; set; }

    [Required, MaxLength(10)]
    public string Moneda { get; set; } = "USD";

    [Required, MaxLength(50)]
    public string MetodoPago { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(300)]
    public string? Concepto { get; set; }

    [MaxLength(50)]
    public string? ProveedorPago { get; set; }

    [MaxLength(200)]
    public string? IdentificadorExterno { get; set; }

    [MaxLength(200)]
    public string? TokenPasarela { get; set; }

    [MaxLength(500)]
    public string? UrlAprobacion { get; set; }

    [MaxLength(50)]
    public string? EstadoExterno { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public int IntentosVerificacion { get; set; } = 0;

    [MaxLength(500)]
    public string? MensajeRespuesta { get; set; }

    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int? ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }

    public int DuenoId { get; set; }
    public Dueno? Dueno { get; set; }

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public DateTime? FechaEliminacion { get; set; }

    [MaxLength(100)]
    public string? EliminadoPor { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
