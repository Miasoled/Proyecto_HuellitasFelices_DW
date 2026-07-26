using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
[Authorize(Roles = "Administrador,Supervisor,Operador")]
public class InventarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInventoryService _inventoryService;
        private const int TamanioPagina = 20;

        public InventarioController(AppDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        // GET: Inventario
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null, int? categoriaId = null)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p => EF.Functions.ILike(p.Nombre, $"%{busqueda}%") || (p.CodigoBarras != null && EF.Functions.ILike(p.CodigoBarras, $"%{busqueda}%")));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var totalRegistros = await query.CountAsync();
            var productos = await query
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanioPagina),
                TotalRegistros = totalRegistros,
                TamanioPagina = TamanioPagina,
                Busqueda = busqueda
            };

            ViewBag.Categorias = new SelectList(
                await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", categoriaId);

            return View(productos);
        }

        // GET: Inventario/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre");
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre");
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,ImagenUrl,PrecioCompra,PrecioVenta,CodigoBarras,UnidadMedida,StockMinimo,CategoriaId,ProveedorId")] Producto producto, int? SucursalId)
        {
            if (ModelState.IsValid)
            {
                producto.Activo = true;
                producto.FechaCreacion = DateTime.UtcNow;
                producto.FechaActualizacion = DateTime.UtcNow;
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                _context.Inventarios.Add(new Inventario
                {
                    ProductoId = producto.Id,
                    SucursalId = SucursalId ?? (await _context.Sucursales.FirstOrDefaultAsync(s => s.Activo))?.Id ?? 1,
                    StockActual = 0,
                    FechaActualizacion = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Producto \"{producto.Nombre}\" registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", producto.ProveedorId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", SucursalId);
            return View(producto);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null || !producto.Activo) return NotFound();
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", producto.ProveedorId);
            var inventarioActual = await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == id);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", inventarioActual?.SucursalId);
            return View(producto);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,ImagenUrl,PrecioCompra,PrecioVenta,CodigoBarras,UnidadMedida,StockMinimo,CategoriaId,ProveedorId")] Producto producto, int? SucursalId)
        {
            if (id != producto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Productos.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Nombre = producto.Nombre;
                existing.Descripcion = producto.Descripcion;
                existing.ImagenUrl = producto.ImagenUrl;
                existing.PrecioCompra = producto.PrecioCompra;
                existing.PrecioVenta = producto.PrecioVenta;
                existing.CodigoBarras = producto.CodigoBarras;
                existing.UnidadMedida = producto.UnidadMedida;
                existing.StockMinimo = producto.StockMinimo;
                existing.CategoriaId = producto.CategoriaId;
                existing.ProveedorId = producto.ProveedorId;
                existing.FechaActualizacion = DateTime.UtcNow;

                if (SucursalId.HasValue)
                {
                    var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == id);
                    if (inventario != null)
                    {
                        inventario.SucursalId = SucursalId.Value;
                        inventario.FechaActualizacion = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Producto \"{existing.Nombre}\" actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", producto.ProveedorId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", SucursalId);
            return View(producto);
        }

        // GET: Inventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // POST: Inventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.Activo = false;
                producto.FechaEliminacion = DateTime.UtcNow;
                producto.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Producto \"{producto.Nombre}\" eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
            if (producto == null) return NotFound();

            var inventarios = await _context.Inventarios
                .Where(i => i.ProductoId == id)
                .ToListAsync();

            ViewBag.Inventarios = inventarios;
            ViewBag.StockTotal = inventarios.Sum(i => i.StockActual);

            return View(producto);
        }

        // GET: Inventario/Movimientos
        public async Task<IActionResult> Movimientos(int pagina = 1, int? productoId = null, DateTime? desde = null, DateTime? hasta = null)
        {
            var movimientos = await _inventoryService.GetMovimientosAsync(productoId, desde, hasta, pagina);
            var total = await _inventoryService.GetTotalMovimientosAsync(productoId, desde, hasta);

            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)TamanioPagina),
                TotalRegistros = total,
                TamanioPagina = TamanioPagina,
                Busqueda = null
            };

            ViewBag.Productos = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", productoId);
            ViewBag.FechaDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = hasta?.ToString("yyyy-MM-dd");

            return View(movimientos);
        }
        // GET: Inventario/Transferir
        public async Task<IActionResult> Transferir(int? productoId = null)
        {
            ViewBag.ProductoId = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", productoId);
            ViewBag.SucursalOrigenId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            ViewBag.SucursalDestinoId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Inventario/Transferir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transferir(int productoId, int sucursalOrigenId, int sucursalDestinoId, int cantidad, string? observacion)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            var origen = await _context.Sucursales.FindAsync(sucursalOrigenId);
            var destino = await _context.Sucursales.FindAsync(sucursalDestinoId);

            if (producto == null || !producto.Activo || origen == null || !origen.Activo || destino == null || !destino.Activo)
            {
                ModelState.AddModelError("", "Datos inválidos.");
            }
            else if (sucursalOrigenId == sucursalDestinoId)
            {
                ModelState.AddModelError("", "La sucursal de origen y destino no pueden ser iguales.");
            }

            if (ModelState.IsValid && producto != null && origen != null && destino != null)
            {
                var resultado = await _inventoryService.TransferirStockAsync(
                    productoId, sucursalOrigenId, sucursalDestinoId, cantidad,
                    User.Identity?.Name, observacion);

                if (resultado)
                {
                    TempData["Mensaje"] = $"Transferencia de {cantidad} unidades de \"{producto.Nombre}\" de {origen.Nombre} a {destino.Nombre} realizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo realizar la transferencia. Verifique el stock disponible.");
                }
            }

            ViewBag.ProductoId = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", productoId);
            ViewBag.SucursalOrigenId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", sucursalOrigenId);
            ViewBag.SucursalDestinoId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", sucursalDestinoId);
            return View();
        }

        // GET: Inventario/Ajustar
        public async Task<IActionResult> Ajustar(int? productoId = null)
        {
            ViewBag.ProductoId = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", productoId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Inventario/Ajustar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajustar(int productoId, int sucursalId, int nuevoStock, string? motivo)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            var sucursal = await _context.Sucursales.FindAsync(sucursalId);

            if (producto == null || !producto.Activo || sucursal == null || !sucursal.Activo)
            {
                ModelState.AddModelError("", "Datos inválidos.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                ModelState.AddModelError("", "El motivo del ajuste es obligatorio.");
            }

            if (ModelState.IsValid)
            {
                var resultado = await _inventoryService.AjustarAsync(
                    productoId, sucursalId, nuevoStock, User.Identity?.Name, motivo);

                if (resultado != null)
                {
                    TempData["Mensaje"] = $"Stock de \"{producto.Nombre}\" ajustado a {nuevoStock} unidades. Motivo: {motivo}";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo realizar el ajuste de inventario.");
                }
            }

            ViewBag.ProductoId = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", productoId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", sucursalId);
            return View();
        }

        // POST: Inventario/Devolver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolver(int productoId, int sucursalId, int cantidad, string? motivo, int? ventaId)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _inventoryService.RegistrarDevolucionAsync(
                productoId, sucursalId, cantidad, ventaId,
                User.Identity?.Name, motivo ?? "Devolución de producto");

            if (resultado)
            {
                TempData["Mensaje"] = $"Devolución de {cantidad} unidades de \"{producto.Nombre}\" registrada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo registrar la devolución.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
