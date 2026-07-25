using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services.PaymentGateway;

namespace HuellitasFelices.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext context,
        IEnumerable<IPaymentGateway> gateways,
        IAuditService auditService,
        IEmailService emailService,
        IInventoryService inventoryService,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _gateways = gateways;
        _auditService = auditService;
        _emailService = emailService;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<Pago> CrearPagoAsync(int ventaId, decimal monto, string proveedor,
        string returnUrl, string cancelUrl)
    {
        var venta = await _context.Ventas
            .Include(v => v.Consulta).ThenInclude(c => c!.Mascota)
            .Include(v => v.Dueno)
            .FirstOrDefaultAsync(v => v.Id == ventaId);

        var numeroPago = $"PAGO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var pago = new Pago
        {
            NumeroPago = numeroPago,
            Monto = monto,
            Moneda = "USD",
            MetodoPago = proveedor,
            Estado = "Pendiente",
            ProveedorPago = proveedor,
            VentaId = ventaId,
            ConsultaId = venta?.ConsultaId,
            DuenoId = venta?.DuenoId ?? 0,
            Concepto = $"Pago consulta - {venta?.Consulta?.Mascota?.Nombre ?? "N/A"}",
            FechaPago = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            IntentosVerificacion = 0
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        var gateway = _gateways.FirstOrDefault(g =>
            g.ProviderName.Equals(proveedor, StringComparison.OrdinalIgnoreCase));

        if (gateway != null)
        {
            var result = await gateway.CreatePaymentAsync(new PaymentRequest
            {
                Monto = monto,
                Moneda = "USD",
                Descripcion = $"Huellitas Felices - {numeroPago}",
                VentaId = ventaId,
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl,
                PhoneNumber = venta?.Dueno?.Telefono,
                Email = venta?.Dueno?.Email
            });

            if (result.Exito)
            {
                pago.TokenPasarela = result.TokenPago;
                pago.UrlAprobacion = result.UrlAprobacion;
                pago.IdentificadorExterno = result.TokenPago;
            }
            else
            {
                pago.Estado = "Fallido";
                pago.MensajeRespuesta = result.MensajeError != null && result.MensajeError.Length > 500
                    ? result.MensajeError.Substring(0, 500)
                    : result.MensajeError;
            }

            pago.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Pago creado: {NumeroPago} por {Monto} {Moneda} via {Proveedor}",
            numeroPago, monto, pago.Moneda, proveedor);

        return pago;
    }

    public async Task<Pago?> ConfirmarPagoAsync(int pagoId)
    {
        var pago = await _context.Pagos
            .Include(p => p.Venta).ThenInclude(v => v!.Consulta).ThenInclude(c => c!.Medicamentos).ThenInclude(cm => cm.Producto)
            .Include(p => p.Venta).ThenInclude(v => v!.Dueno)
            .Include(p => p.Dueno)
            .FirstOrDefaultAsync(p => p.Id == pagoId);

        if (pago == null) return null;

        if (pago.Estado == "Aprobado")
        {
            _logger.LogInformation("Pago {PagoId} ya fue aprobado (idempotencia)", pagoId);
            return pago;
        }

        if (string.IsNullOrEmpty(pago.TokenPasarela)) return pago;

        var gateway = _gateways.FirstOrDefault(g =>
            g.ProviderName.Equals(pago.ProveedorPago, StringComparison.OrdinalIgnoreCase));

        if (gateway == null) return pago;

        pago.IntentosVerificacion++;
        pago.FechaActualizacion = DateTime.UtcNow;

        var verification = await gateway.VerifyPaymentAsync(pago);

        pago.EstadoExterno = verification.Estado;
        pago.MensajeRespuesta = verification.MensajeError != null && verification.MensajeError.Length > 500
            ? verification.MensajeError.Substring(0, 500)
            : verification.MensajeError;

        if (verification.Exito && verification.Aprobado)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                pago.Estado = "Aprobado";
                pago.FechaConfirmacion = DateTime.UtcNow;

                if (pago.Venta != null)
                {
                    pago.Venta.Estado = "Pagada";
                    pago.Venta.MetodoPago = pago.ProveedorPago;
                    pago.Venta.FechaPago = DateTime.UtcNow;

                    if (pago.Venta.Consulta?.Medicamentos != null)
                    {
                        foreach (var med in pago.Venta.Consulta.Medicamentos)
                        {
                            _context.DetallesVenta.Add(new DetalleVenta
                            {
                                VentaId = pago.Venta.Id,
                                ProductoId = med.ProductoId,
                                Cantidad = med.Cantidad,
                                PrecioUnitario = med.PrecioUnitario
                            });
                        }
                    }

                    if (pago.Venta.Consulta != null)
                    {
                        pago.Venta.Consulta.Estado = "Completada";
                        pago.Venta.Consulta.FechaActualizacion = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService.LogAsync("PagoAprobado", "Pago", pago.Id,
                    usuarioId: pago.DuenoId.ToString(),
                    usuarioEmail: pago.Dueno?.Email,
                    valorNuevo: $"Pago {pago.NumeroPago} aprobado via {pago.ProveedorPago}. Monto: ${pago.Monto}");

                if (pago.Dueno?.Email != null && pago.Venta != null)
                {
                    var detalle = pago.Venta.Consulta?.Medicamentos != null
                        ? string.Join(", ", pago.Venta.Consulta.Medicamentos.Select(m => m.Producto?.Nombre ?? ""))
                        : "Consulta veterinaria";

                    await _emailService.EnviarVentaAprobadaAsync(
                        pago.Dueno.Email,
                        pago.Dueno.Nombre,
                        pago.Venta.Id,
                        pago.Monto,
                        detalle);
                }

                _logger.LogInformation("Pago {PagoId} confirmado. Venta {VentaId} actualizada.",
                    pagoId, pago.VentaId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al confirmar pago {PagoId}", pagoId);
                throw;
            }
        }
        else if (verification.Exito && !verification.Aprobado)
        {
            pago.Estado = verification.Estado?.ToLower() switch
            {
                "cancelled" => "Cancelado",
                "failed" => "Fallido",
                "expired" => "Expirado",
                _ => "Fallido"
            };

            pago.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (pago.VentaId > 0)
            {
                try
                {
                    await _inventoryService.RevertirReservaAsync(
                        pago.VentaId, pago.DuenoId.ToString(),
                        $"Pago {pago.Estado}: {pago.NumeroPago}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al revertir reserva para pago {PagoId}", pagoId);
                }
            }

            await _auditService.LogAsync("PagoNoAprobado", "Pago", pago.Id,
                usuarioId: pago.DuenoId.ToString(),
                valorNuevo: $"Pago {pago.NumeroPago} estado: {pago.Estado}. Razon: {pago.MensajeRespuesta}");

            if (pago.Dueno?.Email != null)
            {
                await _emailService.EnviarPagoFallidoAsync(
                    pago.Dueno.Email,
                    pago.Dueno.Nombre,
                    pago.MensajeRespuesta ?? "El pago no fue aprobado");
            }
        }

        return pago;
    }

    public async Task<Pago?> CancelarPagoAsync(int pagoId)
    {
        var pago = await _context.Pagos
            .Include(p => p.Dueno)
            .FirstOrDefaultAsync(p => p.Id == pagoId);

        if (pago == null || pago.Estado != "Pendiente") return pago;

        var gateway = _gateways.FirstOrDefault(g =>
            g.ProviderName.Equals(pago.ProveedorPago, StringComparison.OrdinalIgnoreCase));

        if (gateway != null && !string.IsNullOrEmpty(pago.TokenPasarela))
        {
            var result = await gateway.CancelPaymentAsync(pago.TokenPasarela);
            pago.EstadoExterno = result.Estado;
        }

        pago.Estado = "Cancelado";
        pago.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (pago.VentaId > 0)
        {
            try
            {
                await _inventoryService.RevertirReservaAsync(
                    pago.VentaId, pago.DuenoId.ToString(),
                    $"Pago cancelado: {pago.NumeroPago}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir reserva en cancelación de pago {PagoId}", pagoId);
            }
        }

        await _auditService.LogAsync("PagoCancelado", "Pago", pago.Id,
            usuarioId: pago.DuenoId.ToString(),
            valorNuevo: $"Pago {pago.NumeroPago} cancelado");

        if (pago.Dueno?.Email != null)
        {
            await _emailService.EnviarPagoFallidoAsync(
                pago.Dueno.Email,
                pago.Dueno.Nombre,
                "El pago fue cancelado por el usuario");
        }

        return pago;
    }

    public async Task<Pago?> ObtenerPagoAsync(int pagoId)
    {
        return await _context.Pagos
            .Include(p => p.Venta)
            .Include(p => p.Dueno)
            .FirstOrDefaultAsync(p => p.Id == pagoId);
    }

    public async Task<List<Pago>> ObtenerPagosAsync(int pagina = 1, int tamanioPagina = 20,
        string? busqueda = null, string? estado = null, string? proveedor = null,
        DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Pagos
            .AsNoTracking()
            .Include(p => p.Venta).ThenInclude(v => v!.Consulta).ThenInclude(c => c!.Mascota)
            .Include(p => p.Dueno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.NumeroPago.Contains(busqueda) ||
                (p.Dueno != null && p.Dueno.Nombre.Contains(busqueda)));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(proveedor))
            query = query.Where(p => p.ProveedorPago == proveedor);

        if (desde.HasValue)
            query = query.Where(p => p.FechaCreacion >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(p => p.FechaCreacion <= hasta.Value);

        return await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();
    }

    public async Task<int> TotalPagosAsync(string? busqueda = null, string? estado = null,
        string? proveedor = null, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Pagos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.NumeroPago.Contains(busqueda));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(proveedor))
            query = query.Where(p => p.ProveedorPago == proveedor);

        if (desde.HasValue)
            query = query.Where(p => p.FechaCreacion >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(p => p.FechaCreacion <= hasta.Value);

        return await query.CountAsync();
    }

    public async Task<List<Pago>> ObtenerPagosPorVentaAsync(int ventaId)
    {
        return await _context.Pagos
            .AsNoTracking()
            .Where(p => p.VentaId == ventaId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
    }
}