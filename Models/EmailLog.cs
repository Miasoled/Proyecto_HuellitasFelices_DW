using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class EmailLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Destinatario { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TipoNotificacion { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public DateTime? FechaEnvio { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Enviado, Fallido, Reintentando

        public int Intentos { get; set; } = 0;

        [StringLength(1000)]
        public string? MensajeError { get; set; }

        [StringLength(5000)]
        public string? ContenidoHtml { get; set; }
    }
}
