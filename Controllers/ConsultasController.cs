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
    public class ConsultasController : Controller
    {
        private readonly AppDbContext _context;
        private const int TamanioPagina = 20;

        public ConsultasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Consultas
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Consultas
                .AsNoTracking()
                .Include(c => c.Mascota)
                .Where(c => c.Activo)
                .OrderByDescending(c => c.FechaConsulta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(c => c.Motivo.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var consultas = await consulta
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

            return View(consultas);
        }

        // GET: Consultas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consulta == null)
            {
                return NotFound();
            }

            return View(consulta);
        }

        // GET: Consultas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Consultas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Motivo,Diagnostico,Costo,FechaConsulta,Activo,FechaCreacion")] Consulta consulta, string nombreMascota)
        {
            ModelState.Remove("MascotaId");
            ModelState.Remove("Mascota");

            if (string.IsNullOrWhiteSpace(nombreMascota))
            {
                ModelState.AddModelError("MascotaId", "El nombre de la mascota es obligatorio.");
            }

            if (ModelState.IsValid)
            {
                var mascota = await _context.Mascotas
                    .FirstOrDefaultAsync(m => m.Nombre.ToLower() == nombreMascota.Trim().ToLower() && m.Activo);

                if (mascota == null)
                {
                    mascota = new Mascota
                    {
                        Nombre = nombreMascota.Trim(),
                        Especie = "Perro",
                        Raza = "Mestizo",
                        Edad = 1,
                        Peso = 5.0m,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.Mascotas.Add(mascota);
                    await _context.SaveChangesAsync();
                }

                consulta.MascotaId = mascota.Id;
                _context.Add(consulta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(consulta);
        }

        // GET: Consultas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (consulta == null)
            {
                return NotFound();
            }
            return View(consulta);
        }

        // POST: Consultas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Motivo,Diagnostico,Costo,FechaConsulta,Activo,FechaCreacion")] Consulta consulta, string nombreMascota)
        {
            if (id != consulta.Id)
            {
                return NotFound();
            }

            ModelState.Remove("MascotaId");
            ModelState.Remove("Mascota");

            if (string.IsNullOrWhiteSpace(nombreMascota))
            {
                ModelState.AddModelError("MascotaId", "El nombre de la mascota es obligatorio.");
            }

            if (ModelState.IsValid)
            {
                var mascota = await _context.Mascotas
                    .FirstOrDefaultAsync(m => m.Nombre.ToLower() == nombreMascota.Trim().ToLower() && m.Activo);

                if (mascota == null)
                {
                    mascota = new Mascota
                    {
                        Nombre = nombreMascota.Trim(),
                        Especie = "Perro",
                        Raza = "Mestizo",
                        Edad = 1,
                        Peso = 5.0m,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.Mascotas.Add(mascota);
                    await _context.SaveChangesAsync();
                }

                consulta.MascotaId = mascota.Id;

                try
                {
                    _context.Update(consulta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultaExists(consulta.Id))
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
            return View(consulta);
        }

        // GET: Consultas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consulta == null)
            {
                return NotFound();
            }

            return View(consulta);
        }

        // POST: Consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta != null)
            {
                consulta.Activo = false;
                consulta.FechaEliminacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ConsultaExists(int id)
        {
            return _context.Consultas.Any(e => e.Id == id);
        }
    }
}
