using HuellitasFelices.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuellitasFelices.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carrito;
        private readonly Data.AppDbContext _context;

        public CarritoController(ICarritoService carrito, Data.AppDbContext context)
        {
            _carrito = carrito;
            _context = context;
        }

        // GET: Carrito
        public IActionResult Index()
        {
            var items = _carrito.ObtenerItems();
            ViewBag.Total = _carrito.Total();
            return View(items);
        }

        // POST: Carrito/Agregar
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

        // POST: Carrito/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            _carrito.Eliminar(productoId);
            TempData["CarritoMensaje"] = "Producto eliminado del carrito.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Carrito/Actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Actualizar(int productoId, int cantidad)
        {
            _carrito.ActualizarCantidad(productoId, cantidad);
            return RedirectToAction(nameof(Index));
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vaciar()
        {
            _carrito.Vaciar();
            TempData["CarritoMensaje"] = "Carrito vaciado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
