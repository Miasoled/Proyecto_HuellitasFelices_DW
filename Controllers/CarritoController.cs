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
        private readonly PayPalSettings _paypalSettings;
        private readonly PayPhoneSettings _payphoneSettings;

        public CarritoController(
            ICarritoService carrito,
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IPaymentService paymentService,
            IOptions<PaymentSettings> paymentSettings)
        {
            _carrito = carrito;
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
            _paypalSettings = paymentSettings.Value.PayPal;
            _payphoneSettings = paymentSettings.Value.PayPhone;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var items = _carrito.ObtenerItems();
            ViewBag.Total = _carrito.Total();
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

            string returnUrl, cancelUrl;

            if (metodoPago.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = _paypalSettings.ReturnUrl;
                cancelUrl = _paypalSettings.CancelUrl;
            }
            else
            {
                returnUrl = _payphoneSettings.ReturnUrl;
                cancelUrl = _payphoneSettings.CancelUrl;
            }

            var pago = await _paymentService.CrearPagoAsync(
                venta.Id, totalGeneral, metodoPago, returnUrl, cancelUrl);

            if (pago.Estado == "Fallido")
            {
                _carrito.Vaciar();
                return RedirectToAction("PagoFallido", "Payment",
                    new { motivo = pago.MensajeRespuesta ?? "No se pudo crear el pago con el proveedor" });
            }

            if (!string.IsNullOrEmpty(pago.UrlAprobacion))
            {
                _carrito.Vaciar();
                return Redirect(pago.UrlAprobacion);
            }

            _carrito.Vaciar();
            return RedirectToAction("PagoFallido", "Payment",
                new { motivo = "No se obtuvo URL de aprobación del proveedor de pago" });
        }
    }
}
