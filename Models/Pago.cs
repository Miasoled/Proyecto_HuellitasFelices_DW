using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, 99999)]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; } = string.Empty; // Efectivo, Tarjeta, Transferencia

        [StringLength(50)]
        [Display(Name = "Estado del pago")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagado, Anulado

        // Auditoría y eliminación lógica
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de pago")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Llave foránea
        [Required]
        [Display(Name = "Dueño")]
        public int DuenoId { get; set; }
        public Dueno? Dueno { get; set; }
    }
}