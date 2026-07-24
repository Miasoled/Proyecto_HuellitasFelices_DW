using HuellitasFelices.Data;
using HuellitasFelices.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Services
{
    public class EmailWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailWorker> _logger;

        public EmailWorker(IServiceProvider serviceProvider, ILogger<EmailWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var pendientes = await context.EmailLogs
                        .Where(e => e.Estado == "Pendiente" || (e.Estado == "Fallido" && e.Intentos < 3))
                        .OrderBy(e => e.FechaSolicitud)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    foreach (var log in pendientes)
                    {
                        try
                        {
                            await emailSender.SendEmailAsync(log.Destinatario, log.Asunto, log.ContenidoHtml ?? "");
                            log.Estado = "Enviado";
                            log.FechaEnvio = DateTime.UtcNow;
                            log.Intentos++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Reintento {Intentos} fallido para email {Id}", log.Intentos, log.Id);
                            log.Intentos++;
                            log.MensajeError = ex.Message;
                            log.Estado = log.Intentos >= 3 ? "Fallido" : "Reintentando";
                        }
                    }

                    if (pendientes.Count > 0)
                        await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo del EmailWorker.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("EmailWorker detenido.");
        }
    }
}
