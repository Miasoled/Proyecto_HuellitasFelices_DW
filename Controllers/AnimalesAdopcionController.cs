using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class AnimalesAdopcionController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public AnimalesAdopcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AnimalesAdopcion
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.AnimalesAdopcion
                .AsNoTracking()
                .Where(a => a.Activo)
                .OrderByDescending(a => a.FechaActualizacion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(a => a.Nombre.Contains(busqueda) || a.Especie.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var animales = await consulta
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual    = pagina,
                TotalPaginas    = (int)Math.Ceiling(totalRegistros / (double)TamanioPagina),
                TotalRegistros  = totalRegistros,
                TamanioPagina   = TamanioPagina,
                Busqueda        = busqueda
            };

            return View(animales);
        }

        // GET: AnimalesAdopcion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var animal = await _context.AnimalesAdopcion
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null) return NotFound();

            return View(animal);
        }

        // GET: AnimalesAdopcion/Create
        public IActionResult Create() => View();

        // POST: AnimalesAdopcion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Especie,Raza,EdadAproximada,Descripcion,Disponible,FotoUrl")] AnimalAdopcion animal)
        {
            if (ModelState.IsValid)
            {
                animal.Activo             = true;
                animal.FechaCreacion      = DateTime.UtcNow;
                animal.FechaActualizacion = DateTime.UtcNow;
                _context.Add(animal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        // GET: AnimalesAdopcion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var animal = await _context.AnimalesAdopcion.FindAsync(id);
            if (animal == null) return NotFound();

            return View(animal);
        }

        // POST: AnimalesAdopcion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nombre,Especie,Raza,EdadAproximada,Descripcion,Disponible,Activo,FotoUrl")] AnimalAdopcion animal)
        {
            if (id != animal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Recuperar FechaCreacion original de la BD para evitar el error DateTime
                var original = await _context.AnimalesAdopcion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (original == null) return NotFound();

                animal.FechaCreacion      = original.FechaCreacion;   // ya viene en UTC desde la BD
                animal.FechaActualizacion = DateTime.UtcNow;

                try
                {
                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalAdopcionExists(animal.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        // GET: AnimalesAdopcion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var animal = await _context.AnimalesAdopcion
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null) return NotFound();

            return View(animal);
        }

        // POST: AnimalesAdopcion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animal = await _context.AnimalesAdopcion.FindAsync(id);
            if (animal != null)
            {
                animal.Activo           = false;
                animal.FechaEliminacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalAdopcionExists(int id) =>
            _context.AnimalesAdopcion.Any(e => e.Id == id);
    }
}
