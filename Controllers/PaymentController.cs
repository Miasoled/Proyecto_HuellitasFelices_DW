using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HuellitasFelices.Settings;
using HuellitasFelices.Services.PaymentGateway;

namespace HuellitasFelices.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentController> _logger;
    private readonly PayPalSettings _paypalSettings;
    private readonly PayPhoneSettings _payphoneSettings;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentController(
        IPaymentService paymentService,
        UserManager<IdentityUser> userManager,
        AppDbContext context,
        ILogger<PaymentController> logger,
        IOptions<PaymentSettings> paymentSettings,
        IEnumerable<IPaymentGateway> gateways)
    {
        _paymentService = paymentService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _paypalSettings = paymentSettings.Value.PayPal;
        _payphoneSettings = paymentSettings.Value.PayPhone;
        _gateways = gateways;
    }

    [HttpGet]
    public async Task<IActionResult> Success(string? token, string? PayerID)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("PagoFallido", "Payment", new { motivo = "No se recibió token de PayPal" });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var dueno = await _context.Duenos
            .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
        if (dueno == null) return Forbid();

        var pago = await _context.Pagos
            .Where(p => p.DuenoId == dueno.Id && p.Estado == "Pendiente")
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

        if (pago == null)
            return RedirectToAction("PagoFallido", "Payment", new { motivo = "No se encontró un pago pendiente" });

        try
        {
            var resultado = await _paymentService.ConfirmarPagoAsync(pago.Id);

            if (resultado == null)
                return RedirectToAction("PagoFallido", "Payment", new { motivo = "No se pudo confirmar el pago con el proveedor" });

            if (resultado.Estado == "Aprobado")
                return RedirectToAction("Factura", "Ventas", new { ventaId = resultado.VentaId });

            if (resultado.Estado == "Cancelado")
                return RedirectToAction("PagoCancelado", "Payment");

            return RedirectToAction("PagoFallido", "Payment", new { motivo = resultado.MensajeRespuesta ?? $"Estado inesperado: {resultado.Estado}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en callback de PayPal Success");
            return RedirectToAction("PagoFallido", "Payment", new { motivo = "Error interno al procesar el pago" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Cancel()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var dueno = await _context.Duenos
            .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
        if (dueno == null) return Forbid();

        var pago = await _context.Pagos
            .Where(p => p.DuenoId == dueno.Id && p.Estado == "Pendiente")
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

        if (pago != null)
            await _paymentService.CancelarPagoAsync(pago.Id);

        return View("PagoCancelado");
    }

    [HttpGet]
    public IActionResult PagoFallido(string? motivo)
    {
        ViewBag.Motivo = motivo;
        return View();
    }

    [HttpGet]
    public IActionResult PagoCancelado()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ProcesarPago(int pagoId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var dueno = await _context.Duenos
            .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
        if (dueno == null) return Forbid();

        var pago = await _context.Pagos
            .Include(p => p.Venta)
            .FirstOrDefaultAsync(p => p.Id == pagoId && p.DuenoId == dueno.Id && p.Estado == "Pendiente");

        if (pago == null)
            return RedirectToAction("PagoFallido", "Payment", new { motivo = "No se encontró el pago pendiente para este usuario" });

        ViewBag.ClientId = _paypalSettings.ClientId;
        return View(pago);
    }

    [HttpPost]
    public async Task<IActionResult> CapturePayPalButtonOrderJson([FromBody] PayPalButtonCaptureRequest request)
    {
        if (request == null || request.PagoId <= 0 || string.IsNullOrWhiteSpace(request.PayPalOrderId))
        {
            return Json(new { success = false, message = "Datos de petición no válidos" });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Sesión no válida" });

        var dueno = await _context.Duenos
            .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
        if (dueno == null) return Json(new { success = false, message = "Acceso denegado" });

        var pago = await _context.Pagos
            .FirstOrDefaultAsync(p => p.Id == request.PagoId && p.DuenoId == dueno.Id && p.Estado == "Pendiente");

        if (pago == null)
        {
            return Json(new { success = false, message = "El pago no existe o ya no está pendiente" });
        }

        try
        {
            var resultado = await _paymentService.ConfirmarPagoAsync(pago.Id);

            if (resultado == null)
            {
                return Json(new { success = false, message = "No se pudo confirmar el pago con el proveedor" });
            }

            if (resultado.Estado == "Aprobado")
            {
                return Json(new 
                { 
                    success = true, 
                    redirectUrl = Url.Action("Factura", "Ventas", new { ventaId = resultado.VentaId }) 
                });
            }

            return Json(new { success = false, message = "Ocurrió un error interno al procesar el pago" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al capturar orden PayPal via button");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CambiarMetodoPago(int pagoId, string metodo)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var dueno = await _context.Duenos
            .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
        if (dueno == null) return Forbid();

        var pago = await _context.Pagos
            .Include(p => p.Venta)
            .FirstOrDefaultAsync(p => p.Id == pagoId && p.DuenoId == dueno.Id && p.Estado == "Pendiente");

        if (pago == null) return NotFound();

        if (!pago.ProveedorPago.Equals(metodo, StringComparison.OrdinalIgnoreCase))
        {
            pago.MetodoPago = metodo;
            pago.ProveedorPago = metodo;
            pago.TokenPasarela = null;
            pago.UrlAprobacion = null;
            pago.Estado = "Pendiente";
            pago.FechaActualizacion = DateTime.UtcNow;

            var gateway = _gateways.FirstOrDefault(g =>
                g.ProviderName.Equals(metodo, StringComparison.OrdinalIgnoreCase));

            if (gateway != null)
            {
                var returnUrl = metodo.Equals("PayPal", StringComparison.OrdinalIgnoreCase) 
                    ? _paypalSettings.ReturnUrl 
                    : _payphoneSettings.ReturnUrl;

                var cancelUrl = metodo.Equals("PayPal", StringComparison.OrdinalIgnoreCase)
                    ? _paypalSettings.CancelUrl
                    : _payphoneSettings.CancelUrl;

                var result = await gateway.CreatePaymentAsync(new PaymentRequest
                {
                    Monto = pago.Monto,
                    Moneda = "USD",
                    Descripcion = $"Huellitas Felices - {pago.NumeroPago}",
                    VentaId = pago.VentaId,
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                });

                if (result.Exito)
                {
                    pago.TokenPasarela = result.TokenPago;
                    pago.UrlAprobacion = result.UrlAprobacion;
                }
                else
                {
                    pago.Estado = "Fallido";
                    pago.MensajeRespuesta = result.MensajeError != null && result.MensajeError.Length > 500
                        ? result.MensajeError.Substring(0, 500)
                        : result.MensajeError;
                }

                await _context.SaveChangesAsync();
            }
        }

        if (metodo.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("ProcesarPago", new { pagoId = pago.Id });
        }
        else
        {
            if (!string.IsNullOrEmpty(pago.UrlAprobacion))
            {
                return Redirect(pago.UrlAprobacion);
            }
            return RedirectToAction("PagoFallido", new { motivo = "No se pudo generar la URL de aprobación de la pasarela" });
        }
    }
}

public class PayPalButtonCaptureRequest
{
    public string PayPalOrderId { get; set; } = string.Empty;
    public int PagoId { get; set; }
}
