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
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Pagos
                .AsNoTracking()
                .Include(p => p.Dueno)
                .Where(p => p.Activo)
                .OrderByDescending(p => p.FechaPago)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(p => p.Estado.Contains(busqueda) || p.MetodoPago.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var pagos = await consulta
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

            return View(pagos);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Dueno)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre");
            return View();
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Monto,MetodoPago,Estado,FechaPago,DuenoId")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                pago.FechaCreacion = DateTime.UtcNow;
                pago.FechaActualizacion = DateTime.UtcNow;
                pago.Activo = true;
                _context.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre", pago.DuenoId);
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null) return NotFound();

            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre", pago.DuenoId);
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Monto,MetodoPago,Estado,FechaPago,Activo,FechaCreacion,DuenoId")] Pago pago)
        {
            if (id != pago.Id) return NotFound();

            if (ModelState.IsValid)
            {
                pago.FechaActualizacion = DateTime.UtcNow;
                _context.Update(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre", pago.DuenoId);
            return View(pago);
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pago = await _context.Pagos
                .Include(p => p.Dueno)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (pago == null) return NotFound();

            return View(pago);
        }

        // POST: Pagos/Delete/5 — Eliminación lógica
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                pago.Activo = false;
                pago.FechaEliminacion = DateTime.UtcNow;
                pago.FechaActualizacion = DateTime.UtcNow;
                _context.Update(pago);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.Id == id);
        }
    }
}