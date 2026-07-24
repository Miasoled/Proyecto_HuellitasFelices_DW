using HuellitasFelices.Data;
using HuellitasFelices.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace HuellitasFelices.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEmailSender emailSender, AppDbContext context, ILogger<EmailService> logger)
        {
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
        }

        public async Task EnviarConfirmacionCorreoAsync(string email, string nombreUsuario, string confirmationLink)
        {
            await SendTrackedAsync(email, "Confirma tu correo electrónico - Huellitas Felices",
                "ConfirmacionCorreo",
                EmailTemplates.WelcomeTemplate(email, confirmationLink));
        }

        public async Task EnviarRecuperarPasswordAsync(string email, string resetLink)
        {
            await SendTrackedAsync(email, "Restablecer contraseña - Huellitas Felices",
                "RecuperarPassword",
                EmailTemplates.ForgotPasswordTemplate(resetLink));
        }

        public async Task EnviarCambioPasswordAsync(string email, string nombreUsuario)
        {
            await SendTrackedAsync(email, "Contraseña cambiada - Huellitas Felices",
                "CambioPassword",
                EmailTemplates.PasswordChangedTemplate(nombreUsuario));
        }

        public async Task EnviarCuentaBloqueadaAsync(string email, string nombreUsuario)
        {
            await SendTrackedAsync(email, "Cuenta bloqueada por seguridad - Huellitas Felices",
                "CuentaBloqueada",
                EmailTemplates.AccountLockedTemplate(nombreUsuario));
        }

        public async Task EnviarActivacionMFAAsync(string email, string nombreUsuario)
        {
            await SendTrackedAsync(email, "Autenticación multifactor activada - Huellitas Felices",
                "ActivacionMFA",
                EmailTemplates.MfaActivatedTemplate(nombreUsuario));
        }

        public async Task EnviarVentaAprobadaAsync(string email, string nombreUsuario, int ventaId, decimal total, string detalle)
        {
            await SendTrackedAsync(email, $"Venta #{ventaId} confirmada - Huellitas Felices",
                "VentaAprobada",
                EmailTemplates.SaleApprovedTemplate(nombreUsuario, ventaId, total, detalle));
        }

        public async Task EnviarPagoFallidoAsync(string email, string nombreUsuario, string razon)
        {
            await SendTrackedAsync(email, "Pago no procesado - Huellitas Felices",
                "PagoFallido",
                EmailTemplates.PaymentFailedTemplate(nombreUsuario, razon));
        }

        public async Task EnviarInventarioCriticoAsync(string email, string nombreProducto, int stockActual, int stockMinimo)
        {
            await SendTrackedAsync(email, $"Alerta: Stock bajo de {nombreProducto} - Huellitas Felices",
                "InventarioCritico",
                EmailTemplates.LowStockTemplate(nombreProducto, stockActual, stockMinimo));
        }

        public async Task EnviarAdopcionAsync(string email, string nombreSolicitante, string nombreAnimal, string especie, string codigo)
        {
            await SendTrackedAsync(email, "Solicitud de adopción recibida - Huellitas Felices",
                "Adopcion",
                EmailTemplates.AdoptionTemplate(nombreSolicitante, nombreAnimal, especie, codigo));
        }

        private async Task SendTrackedAsync(string email, string subject, string tipoNotificacion, string htmlContent)
        {
            var log = new EmailLog
            {
                Destinatario = email,
                Asunto = subject,
                TipoNotificacion = tipoNotificacion,
                ContenidoHtml = htmlContent,
                FechaSolicitud = DateTime.UtcNow,
                Estado = "Pendiente",
                Intentos = 0
            };

            _context.EmailLogs.Add(log);
            await _context.SaveChangesAsync();

            try
            {
                await _emailSender.SendEmailAsync(email, subject, htmlContent);

                log.Estado = "Enviado";
                log.FechaEnvio = DateTime.UtcNow;
                log.Intentos = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Email} tipo {Tipo}", email, tipoNotificacion);
                log.Estado = "Fallido";
                log.MensajeError = ex.Message;
                log.Intentos = 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}
