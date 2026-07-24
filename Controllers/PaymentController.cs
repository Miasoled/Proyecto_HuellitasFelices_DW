using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        UserManager<IdentityUser> userManager,
        AppDbContext context,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
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

        return View();
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
}
