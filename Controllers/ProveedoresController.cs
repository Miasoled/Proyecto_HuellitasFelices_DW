using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador,Supervisor,Operador")]
    public class ProveedoresController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var query = _context.Proveedores
                .AsNoTracking()
                .Include(p => p.Productos)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(p => p.Nombre.Contains(busqueda) || p.RUC.Contains(busqueda));

            var totalRegistros = await query.CountAsync();
            var proveedores = await query
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

            return View(proveedores);
        }

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Telefono,Email,Direccion,RUC")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                proveedor.Activo = true;
                proveedor.FechaCreacion = DateTime.UtcNow;
                proveedor.FechaActualizacion = DateTime.UtcNow;
                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null || !proveedor.Activo) return NotFound();
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Telefono,Email,Direccion,RUC")] Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Proveedores.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Nombre = proveedor.Nombre;
                existing.Telefono = proveedor.Telefono;
                existing.Email = proveedor.Email;
                existing.Direccion = proveedor.Direccion;
                existing.RUC = proveedor.RUC;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                proveedor.Activo = false;
                proveedor.FechaEliminacion = DateTime.UtcNow;
                proveedor.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
