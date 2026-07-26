using HuellitasFelices.Data;
using HuellitasFelices.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Services;

public class PaymentExpirationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PaymentSettings _settings;
    private readonly ILogger<PaymentExpirationWorker> _logger;

    public PaymentExpirationWorker(
        IServiceProvider serviceProvider,
        IOptions<PaymentSettings> options,
        ILogger<PaymentExpirationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromMinutes(Math.Max(1, _settings.PendingPaymentWorkerIntervalMinutes));
        _logger.LogInformation(
            "PaymentExpirationWorker iniciado. Expiración: {Minutos} min; revisión: {Intervalo} min.",
            _settings.PendingPaymentExpirationMinutes, intervalo.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirarPagosPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al expirar pagos pendientes.");
            }

            await Task.Delay(intervalo, stoppingToken);
        }
    }

    private async Task ExpirarPagosPendientesAsync(CancellationToken stoppingToken)
    {
        var minutosExpiracion = Math.Max(1, _settings.PendingPaymentExpirationMinutes);
        var limite = DateTime.UtcNow.AddMinutes(-minutosExpiracion);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var pagosVencidos = await context.Pagos
            .AsNoTracking()
            .Where(p => p.Estado == "Pendiente" && p.FechaCreacion <= limite)
            // Los pagos recientes tienen prioridad: no deben quedar detrás de datos históricos del seeder.
            .OrderByDescending(p => p.FechaCreacion)
            .Take(50)
            .Select(p => p.Id)
            .ToListAsync(stoppingToken);

        foreach (var pagoId in pagosVencidos)
        {
            stoppingToken.ThrowIfCancellationRequested();

            // No esperamos a una API externa: la prioridad es liberar la reserva local.
            var pago = await paymentService.CancelarPagoAsync(pagoId, cancelarEnPasarela: false);
            if (pago?.Estado != "Cancelado")
                continue;

            await auditService.LogAsync(
                accion: "PagoExpirado",
                entidad: "Pago",
                entidadId: pago.Id,
                usuarioId: pago.DuenoId.ToString(),
                valorNuevo: $"Pago expirado automáticamente después de {minutosExpiracion} minutos sin confirmación.");

            _logger.LogInformation("Pago pendiente {PagoId} expirado automáticamente.", pago.Id);
        }
    }
}
