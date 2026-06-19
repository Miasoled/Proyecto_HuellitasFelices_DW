using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class SolicitudAdopcion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del solicitante es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre del solicitante")]
        public string NombreSolicitante { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Llave foránea
        [Required]
        [Display(Name = "Animal")]
        public int AnimalAdopcionId { get; set; }
        public AnimalAdopcion? AnimalAdopcion { get; set; }
    }
}