using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
[Authorize(Roles = "Administrador,Supervisor")]
public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private const int TamanioPagina = 20;

        public CategoriasController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Categorias
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var query = _context.Categorias
                .AsNoTracking()
                .Include(c => c.Productos)
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(c => EF.Functions.ILike(c.Nombre, $"%{busqueda}%") || (c.Descripcion != null && EF.Functions.ILike(c.Descripcion, $"%{busqueda}%")));

            var totalRegistros = await query.CountAsync();
            var categorias = await query
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

            return View(categorias);
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.Activo = true;
                categoria.FechaCreacion = DateTime.UtcNow;
                categoria.FechaActualizacion = DateTime.UtcNow;
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null || !categoria.Activo) return NotFound();
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Categorias.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Nombre = categoria.Nombre;
                existing.Descripcion = categoria.Descripcion;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                categoria.Activo = false;
                categoria.FechaEliminacion = DateTime.UtcNow;
                categoria.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("EliminacionLogica", "Categoria", categoria.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorAnterior: "Registro activo",
                    valorNuevo: "Registro eliminado lógicamente");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
