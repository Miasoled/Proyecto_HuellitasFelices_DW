using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;
using HuellitasFelices.Settings;
using Microsoft.Extensions.Options;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly IInventoryService _inventoryService;
        private readonly PayPalSettings _paypalSettings;
        private readonly ILogger<VentasController> _logger;

        public VentasController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IPaymentService paymentService,
            IInventoryService inventoryService,
            IOptions<PaymentSettings> paymentSettings,
            ILogger<VentasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
            _inventoryService = inventoryService;
            _paypalSettings = paymentSettings.Value.PayPal;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Pagar(int? consultaId)
        {
            if (consultaId == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(c => c.Id == consultaId && c.Activo);

            if (consulta == null) return NotFound();
            if (consulta.Mascota?.DuenoId != dueno.Id) return Forbid();
            if (consulta.Estado != "EnRevision") return RedirectToAction("MiPanel", "Account");
            if (consulta.Venta != null) return RedirectToAction("MiPanel", "Account");

            return View(consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PagarConfirm(int consultaId, string metodoPago)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
                .Include(c => c.Venta)
                .FirstOrDefaultAsync(c => c.Id == consultaId && c.Activo);

            if (consulta == null) return NotFound();
            if (consulta.Mascota?.DuenoId != dueno.Id) return Forbid();
            if (consulta.Estado != "EnRevision" || consulta.Venta != null)
                return RedirectToAction("MiPanel", "Account");

            var totalMedicamentos = consulta.Medicamentos?.Sum(m => m.Subtotal) ?? 0;
            var totalGeneral = consulta.Costo + totalMedicamentos;
            var numeroVenta = $"VTA-{DateTime.UtcNow:yyyyMMdd}-{consultaId}";

            var venta = new Venta
            {
                NumeroVenta = numeroVenta,
                ConsultaId = consultaId,
                DuenoId = dueno.Id,
                TotalConsulta = consulta.Costo,
                TotalMedicamentos = totalMedicamentos,
                Estado = "Pendiente",
                MetodoPago = metodoPago,
                FechaVenta = DateTime.UtcNow,
                Activo = true
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            if (consulta.Medicamentos != null && consulta.Medicamentos.Count > 0)
            {
                var itemsParaReservar = consulta.Medicamentos
                    .Select(m => (m.ProductoId, m.Cantidad))
                    .ToList();

                var reserva = await _inventoryService.ReservarStockParaVentaAsync(
                    venta.Id, itemsParaReservar, user.Id);

                if (reserva == null || reserva.Count == 0)
                {
                    _context.Ventas.Remove(venta);
                    await _context.SaveChangesAsync();
                    TempData["ErrorMessage"] = "No hay stock suficiente para los medicamentos recetados.";
                    return RedirectToAction("Pagar", new { consultaId });
                }
            }

            string returnUrl = _paypalSettings.ReturnUrl;
            string cancelUrl = _paypalSettings.CancelUrl;

            var pago = await _paymentService.CrearPagoAsync(
                venta.Id, totalGeneral, metodoPago, returnUrl, cancelUrl);

            if (pago.Estado == "Fallido")
            {
                if (consulta.Medicamentos != null && consulta.Medicamentos.Count > 0)
                {
                    await _inventoryService.RevertirReservaAsync(venta.Id, user.Id, "Fallo al crear pago");
                }
                return RedirectToAction("PagoFallido", "Payment");
            }

            if (!string.IsNullOrEmpty(pago.UrlAprobacion))
                return Redirect(pago.UrlAprobacion);

            if (consulta.Medicamentos != null && consulta.Medicamentos.Count > 0)
            {
                await _inventoryService.RevertirReservaAsync(venta.Id, user.Id, "No se obtuvo URL de aprobacion");
            }
            return RedirectToAction("PagoFallido", "Payment");
        }

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Factura(int? ventaId)
        {
            if (ventaId == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var venta = await _context.Ventas
                .Include(v => v.Consulta).ThenInclude(c => c!.Mascota)
                .Include(v => v.Consulta).ThenInclude(c => c!.Veterinario)
                .Include(v => v.Dueno)
                .Include(v => v.Detalles).ThenInclude(dv => dv.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId && v.Activo);

            if (venta == null) return NotFound();
            if (venta.DuenoId != dueno.Id) return Forbid();

            var pago = await _context.Pagos
                .Where(p => p.VentaId == ventaId && p.Estado == "Aprobado")
                .OrderByDescending(p => p.FechaConfirmacion)
                .FirstOrDefaultAsync();

            ViewBag.Pago = pago;

            return View(venta);
        }
    }
}