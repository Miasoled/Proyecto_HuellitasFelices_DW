using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador,Supervisor,Operador,Auditor")]
    public class SucursalesController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public SucursalesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Sucursales
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var query = _context.Sucursales
                .AsNoTracking()
                .Where(s => s.Activo)
                .OrderBy(s => s.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(s => EF.Functions.ILike(s.Nombre, $"%{busqueda}%") ||
                    (s.Ciudad != null && EF.Functions.ILike(s.Ciudad, $"%{busqueda}%")));

            var totalRegistros = await query.CountAsync();
            var sucursales = await query
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

            return View(sucursales);
        }

        // GET: Sucursales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Activo);
            if (sucursal == null) return NotFound();
            return View(sucursal);
        }

        // GET: Sucursales/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sucursales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("Nombre,Direccion,Telefono,Email,Ciudad,EsPrincipal")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                sucursal.Activo = true;
                sucursal.FechaCreacion = DateTime.UtcNow;
                sucursal.FechaActualizacion = DateTime.UtcNow;
                _context.Sucursales.Add(sucursal);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Sucursal \"{sucursal.Nombre}\" creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        // GET: Sucursales/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal == null || !sucursal.Activo) return NotFound();
            return View(sucursal);
        }

        // POST: Sucursales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Direccion,Telefono,Email,Ciudad,EsPrincipal")] Sucursal sucursal)
        {
            if (id != sucursal.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Sucursales.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Nombre = sucursal.Nombre;
                existing.Direccion = sucursal.Direccion;
                existing.Telefono = sucursal.Telefono;
                existing.Email = sucursal.Email;
                existing.Ciudad = sucursal.Ciudad;
                existing.EsPrincipal = sucursal.EsPrincipal;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Sucursal \"{existing.Nombre}\" actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        // GET: Sucursales/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sucursal = await _context.Sucursales
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Activo);
            if (sucursal == null) return NotFound();
            return View(sucursal);
        }

        // POST: Sucursales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal != null)
            {
                sucursal.Activo = false;
                sucursal.FechaEliminacion = DateTime.UtcNow;
                sucursal.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Sucursal \"{sucursal.Nombre}\" eliminada correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
