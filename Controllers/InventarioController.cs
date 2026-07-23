using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador")]
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
                query = query.Where(p => p.Nombre.Contains(busqueda) || (p.CodigoBarras != null && p.CodigoBarras.Contains(busqueda)));

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
            return View();
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,PrecioCompra,PrecioVenta,CodigoBarras,UnidadMedida,StockMinimo,CategoriaId,ProveedorId")] Producto producto)
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
                    StockActual = 0,
                    FechaActualizacion = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Producto \"{producto.Nombre}\" registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", producto.ProveedorId);
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
            return View(producto);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,PrecioCompra,PrecioVenta,CodigoBarras,UnidadMedida,StockMinimo,CategoriaId,ProveedorId")] Producto producto)
        {
            if (id != producto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Productos.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Nombre = producto.Nombre;
                existing.Descripcion = producto.Descripcion;
                existing.PrecioCompra = producto.PrecioCompra;
                existing.PrecioVenta = producto.PrecioVenta;
                existing.CodigoBarras = producto.CodigoBarras;
                existing.UnidadMedida = producto.UnidadMedida;
                existing.StockMinimo = producto.StockMinimo;
                existing.CategoriaId = producto.CategoriaId;
                existing.ProveedorId = producto.ProveedorId;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Producto \"{existing.Nombre}\" actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", producto.ProveedorId);
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
    }
}
