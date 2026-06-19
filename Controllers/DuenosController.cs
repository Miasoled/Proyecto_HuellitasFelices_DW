using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class DuenosController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public DuenosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Duenos
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Duenos
                .AsNoTracking()
                .Where(d => d.Activo)
                .OrderBy(d => d.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(d => d.Nombre.Contains(busqueda) || d.Email!.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var duenos = await consulta
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

            return View(duenos);
        }

        // GET: Duenos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dueno == null)
            {
                return NotFound();
            }

            return View(dueno);
        }

        // GET: Duenos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Duenos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Telefono,Email,Direccion,Activo,FechaCreacion")] Dueno dueno)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dueno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dueno);
        }

        // GET: Duenos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno == null)
            {
                return NotFound();
            }
            return View(dueno);
        }

        // POST: Duenos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Telefono,Email,Direccion,Activo,FechaCreacion")] Dueno dueno)
        {
            if (id != dueno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dueno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DuenoExists(dueno.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dueno);
        }

        // GET: Duenos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dueno = await _context.Duenos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dueno == null)
            {
                return NotFound();
            }

            return View(dueno);
        }

        // POST: Duenos/Delete/5 — Solo Administrador y Supervisor
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno != null)
            {
                dueno.Activo = false;
                dueno.FechaEliminacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DuenoExists(int id)
        {
            return _context.Duenos.Any(e => e.Id == id);
        }
    }
}
