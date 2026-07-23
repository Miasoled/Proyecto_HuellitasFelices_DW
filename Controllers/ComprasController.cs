using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador,Supervisor,Operador")]
    public class ComprasController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Compras
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null, string? estado = null)
        {
            var query = _context.Compras
                .AsNoTracking()
                .Include(c => c.Proveedor)
                .Include(c => c.Sucursal)
                .Where(c => c.Activo)
                .OrderByDescending(c => c.FechaCompra)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(c => c.NumeroCompra.Contains(busqueda) || (c.Observacion != null && c.Observacion.Contains(busqueda)));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado == estado);

            var totalRegistros = await query.CountAsync();
            var compras = await query
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

            ViewBag.Estados = new SelectList(new[]
            {
                new { Value = "", Text = "Todos" },
                new { Value = "Pendiente", Text = "Pendiente" },
                new { Value = "Recibida", Text = "Recibida" },
                new { Value = "Cancelada", Text = "Cancelada" }
            }, "Value", "Text", estado);

            ViewBag.Busqueda = busqueda;
            ViewBag.EstadoSeleccionado = estado;

            return View(compras);
        }

        // GET: Compras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Sucursal)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (compra == null) return NotFound();
            return View(compra);
        }

        // GET: Compras/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre");
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Compras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumeroCompra,Total,Estado,FechaCompra,Observacion,ProveedorId,SucursalId")] Compra compra)
        {
            if (ModelState.IsValid)
            {
                compra.Activo = true;
                compra.FechaCreacion = DateTime.UtcNow;
                compra.FechaActualizacion = DateTime.UtcNow;
                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", compra.ProveedorId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", compra.SucursalId);
            return View(compra);
        }

        // GET: Compras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var compra = await _context.Compras.FindAsync(id);
            if (compra == null || !compra.Activo) return NotFound();
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", compra.ProveedorId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", compra.SucursalId);
            return View(compra);
        }

        // POST: Compras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroCompra,Total,Estado,FechaCompra,Observacion,ProveedorId,SucursalId")] Compra compra)
        {
            if (id != compra.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Compras.FindAsync(id);
                if (existing == null) return NotFound();
                existing.NumeroCompra = compra.NumeroCompra;
                existing.Total = compra.Total;
                existing.Estado = compra.Estado;
                existing.FechaCompra = compra.FechaCompra;
                existing.Observacion = compra.Observacion;
                existing.ProveedorId = compra.ProveedorId;
                existing.SucursalId = compra.SucursalId;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ProveedorId = new SelectList(await _context.Proveedores.Where(p => p.Activo).ToListAsync(), "Id", "Nombre", compra.ProveedorId);
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", compra.SucursalId);
            return View(compra);
        }

        // GET: Compras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Sucursal)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (compra == null) return NotFound();
            return View(compra);
        }

        // POST: Compras/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra != null)
            {
                compra.Activo = false;
                compra.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
