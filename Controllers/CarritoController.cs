using HuellitasFelices.Models;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using Microsoft.Extensions.Options;
using HuellitasFelices.Settings;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carrito;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly IInventoryService _inventoryService;
        private readonly PayPalSettings _paypalSettings;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(
            ICarritoService carrito,
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IPaymentService paymentService,
            IInventoryService inventoryService,
            IOptions<PaymentSettings> paymentSettings,
            ILogger<CarritoController> logger)
        {
            _carrito = carrito;
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
            _inventoryService = inventoryService;
            _paypalSettings = paymentSettings.Value.PayPal;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var items = _carrito.ObtenerItems();
            ViewBag.Total = _carrito.Total();
            ViewBag.ClientId = _paypalSettings.ClientId;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int productoId, int cantidad = 1, string? returnUrl = null)
        {
            var producto = _context.Productos
                .FirstOrDefault(p => p.Id == productoId && p.Activo);

            if (producto == null)
                return NotFound();

            var stock = _context.Inventarios
                .Where(i => i.ProductoId == productoId)
                .Sum(i => i.StockActual);

            _carrito.Agregar(
                producto.Id,
                producto.Nombre,
                producto.PrecioVenta,
                producto.Categoria?.Nombre,
                producto.UnidadMedida,
                stock,
                cantidad
            );

            TempData["CarritoMensaje"] = $"\"{producto.Nombre}\" agregado al carrito.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            _carrito.Eliminar(productoId);
            TempData["CarritoMensaje"] = "Producto eliminado del carrito.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Actualizar(int productoId, int cantidad)
        {
            _carrito.ActualizarCantidad(productoId, cantidad);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vaciar()
        {
            _carrito.Vaciar();
            TempData["CarritoMensaje"] = "Carrito vaciado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Pagar(string metodoPago = "PayPal")
        {
            var items = _carrito.ObtenerItems();
            if (!items.Any())
            {
                TempData["CarritoMensaje"] = "El carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Forbid();

            var totalGeneral = _carrito.Total();

            var venta = new Venta
            {
                NumeroVenta = $"VTA-TIENDA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                ConsultaId = null,
                DuenoId = dueno.Id,
                TotalConsulta = 0,
                TotalMedicamentos = totalGeneral,
                Estado = "Pendiente",
                MetodoPago = metodoPago,
                FechaVenta = DateTime.UtcNow,
                Activo = true
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            var itemsParaReservar = items.Select(i => (i.ProductoId, i.Cantidad)).ToList();
            var reserva = await _inventoryService.ReservarStockParaVentaAsync(
                venta.Id, itemsParaReservar, user.Id);

            if (reserva == null || reserva.Count == 0)
            {
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
                _carrito.Vaciar();
                TempData["CarritoMensaje"] = "No hay stock suficiente para algunos productos del carrito.";
                return RedirectToAction(nameof(Index));
            }

            string returnUrl = _paypalSettings.ReturnUrl;
            string cancelUrl = _paypalSettings.CancelUrl;

            var pago = await _paymentService.CrearPagoAsync(
                venta.Id, totalGeneral, metodoPago, returnUrl, cancelUrl);

            if (pago.Estado == "Fallido")
            {
                await _inventoryService.RevertirReservaAsync(venta.Id, user.Id, "Fallo al crear pago");
                _carrito.Vaciar();
                return RedirectToAction("PagoFallido", "Payment",
                    new { motivo = pago.MensajeRespuesta ?? "No se pudo crear el pago con el proveedor" });
            }

            if (!string.IsNullOrEmpty(pago.UrlAprobacion))
            {
                _carrito.Vaciar();
                return Redirect(pago.UrlAprobacion);
            }

            await _inventoryService.RevertirReservaAsync(venta.Id, user.Id, "No se obtuvo URL de aprobacion");
            _carrito.Vaciar();
            return RedirectToAction("PagoFallido", "Payment",
                new { motivo = "No se obtuvo URL de aprobación del proveedor de pago" });
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CrearPagoBotonJson()
        {
            var items = _carrito.ObtenerItems();
            if (!items.Any())
            {
                return Json(new { success = false, message = "El carrito está vacío" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Sesión no válida" });

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(d => d.Email != null && d.Email == user.Email && d.Activo);
            if (dueno == null) return Json(new { success = false, message = "Acceso denegado" });

            var totalGeneral = _carrito.Total();

            var venta = new Venta
            {
                NumeroVenta = $"VTA-TIENDA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                ConsultaId = null,
                DuenoId = dueno.Id,
                TotalConsulta = 0,
                TotalMedicamentos = totalGeneral,
                Estado = "Pendiente",
                MetodoPago = "PayPal",
                FechaVenta = DateTime.UtcNow,
                Activo = true
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            var itemsParaReservar = items.Select(i => (i.ProductoId, i.Cantidad)).ToList();
            var reserva = await _inventoryService.ReservarStockParaVentaAsync(
                venta.Id, itemsParaReservar, user.Id);

            if (reserva == null || reserva.Count == 0)
            {
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = "No hay stock suficiente para algunos productos" });
            }

            var returnUrl = _paypalSettings.ReturnUrl;
            var cancelUrl = _paypalSettings.CancelUrl;

            var pago = await _paymentService.CrearPagoAsync(
                venta.Id, totalGeneral, "PayPal", returnUrl, cancelUrl);

            if (pago.Estado == "Fallido")
            {
                await _inventoryService.RevertirReservaAsync(venta.Id, user.Id, "Fallo al crear pago PayPal");
                return Json(new { success = false, message = pago.MensajeRespuesta ?? "No se pudo crear el pago con el proveedor" });
            }

            _carrito.Vaciar();

            return Json(new
            {
                success = true,
                paypalOrderId = pago.TokenPasarela,
                pagoId = pago.Id
            });
        }
    }
}