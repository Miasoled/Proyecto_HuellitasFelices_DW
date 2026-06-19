using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador,Supervisor,Operador")]
    public class TratamientosController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public TratamientosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tratamientos
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Tratamientos
                .AsNoTracking()
                .Include(t => t.Consulta)
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(t => t.Nombre.Contains(busqueda) || t.Medicamento!.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var tratamientos = await consulta
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

            return View(tratamientos);
        }

        // GET: Tratamientos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tratamiento = await _context.Tratamientos
                .Include(t => t.Consulta)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (tratamiento == null) return NotFound();

            return View(tratamiento);
        }

        // GET: Tratamientos/Create
        public IActionResult Create()
        {
            ViewData["ConsultaId"] = new SelectList(_context.Consultas, "Id", "Motivo");
            return View();
        }

        // POST: Tratamientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Costo,Medicamento,ConsultaId")] Tratamiento tratamiento)
        {
            if (ModelState.IsValid)
            {
                tratamiento.FechaCreacion = DateTime.UtcNow;
                tratamiento.FechaActualizacion = DateTime.UtcNow;
                tratamiento.Activo = true;
                _context.Add(tratamiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConsultaId"] = new SelectList(_context.Consultas, "Id", "Motivo", tratamiento.ConsultaId);
            return View(tratamiento);
        }

        // GET: Tratamientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tratamiento = await _context.Tratamientos.FindAsync(id);
            if (tratamiento == null) return NotFound();

            ViewData["ConsultaId"] = new SelectList(_context.Consultas, "Id", "Motivo", tratamiento.ConsultaId);
            return View(tratamiento);
        }

        // POST: Tratamientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Costo,Medicamento,Activo,FechaCreacion,ConsultaId")] Tratamiento tratamiento)
        {
            if (id != tratamiento.Id) return NotFound();

            if (ModelState.IsValid)
            {
                tratamiento.FechaActualizacion = DateTime.UtcNow;
                _context.Update(tratamiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConsultaId"] = new SelectList(_context.Consultas, "Id", "Motivo", tratamiento.ConsultaId);
            return View(tratamiento);
        }

        // GET: Tratamientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tratamiento = await _context.Tratamientos
                .Include(t => t.Consulta)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (tratamiento == null) return NotFound();

            return View(tratamiento);
        }

        // POST: Tratamientos/Delete/5 — Eliminación lógica
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tratamiento = await _context.Tratamientos.FindAsync(id);
            if (tratamiento != null)
            {
                tratamiento.Activo = false;
                tratamiento.FechaEliminacion = DateTime.UtcNow;
                tratamiento.FechaActualizacion = DateTime.UtcNow;
                _context.Update(tratamiento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TratamientoExists(int id)
        {
            return _context.Tratamientos.Any(e => e.Id == id);
        }
    }
}