using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class SolicitudesAdopcionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private const int TamanioPagina = 20;

        public SolicitudesAdopcionController(AppDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: SolicitudesAdopcion
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.SolicitudesAdopcion
                .AsNoTracking()
                .Include(s => s.AnimalAdopcion)
                .Where(s => s.Activo)
                .OrderByDescending(s => s.FechaSolicitud)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(s => s.NombreSolicitante.Contains(busqueda) || s.Estado.Contains(busqueda));

            var totalRegistros = await consulta.CountAsync();
            var solicitudes = await consulta
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

            return View(solicitudes);
        }

        // GET: SolicitudesAdopcion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion
                .Include(s => s.AnimalAdopcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }

            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Create
        public async Task<IActionResult> Create(int? animalId)
        {
            if (animalId == null)
            {
                TempData["Error"] = "Debe seleccionar un animal para iniciar la solicitud de adopción.";
                return RedirectToAction("Index", "AnimalesAdopcion");
            }

            var animal = await _context.AnimalesAdopcion.FindAsync(animalId);
            if (animal == null || !animal.Disponible || !animal.Activo)
            {
                TempData["Error"] = "El animal seleccionado no está disponible para adopción.";
                return RedirectToAction("Index", "AnimalesAdopcion");
            }

            // Obtener datos del cliente logueado para pre-rellenar
            string userEmail = User.Identity?.Name ?? string.Empty;
            var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);

            var model = new SolicitudAdopcion
            {
                AnimalAdopcionId = animal.Id,
                AnimalAdopcion = animal,
                NombreSolicitante = dueno?.Nombre ?? string.Empty,
                Telefono = dueno?.Telefono ?? string.Empty,
                Email = dueno?.Email ?? userEmail,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            ViewBag.Animal = animal;
            ViewData["AnimalAdopcionId"] = new SelectList(new List<AnimalAdopcion> { animal }, "Id", "Nombre", animal.Id);
            return View(model);
        }

        // POST: SolicitudesAdopcion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreSolicitante,Telefono,Email,AnimalAdopcionId,Motivo")] SolicitudAdopcion solicitudAdopcion)
        {
            // Forzar el estado a Pendiente y auditorías
            solicitudAdopcion.Estado = "Pendiente";
            solicitudAdopcion.FechaSolicitud = DateTime.UtcNow;
            solicitudAdopcion.FechaCreacion = DateTime.UtcNow;
            solicitudAdopcion.FechaActualizacion = DateTime.UtcNow;
            solicitudAdopcion.Activo = true;

            ModelState.Remove("Estado");

            if (ModelState.IsValid)
            {
                _context.Add(solicitudAdopcion);
                await _context.SaveChangesAsync();

                try
                {
                    if (!string.IsNullOrEmpty(solicitudAdopcion.Email))
                    {
                        var animal = await _context.AnimalesAdopcion.FindAsync(solicitudAdopcion.AnimalAdopcionId);
                        string animalNombre = animal?.Nombre ?? "Nuestra Mascota";
                        string animalEspecie = animal?.Especie ?? "Mascota";

                        await _emailSender.SendEmailAsync(
                            solicitudAdopcion.Email,
                            $"Solicitud de adopción recibida - Huellitas Felices",
                            EmailTemplates.AdoptionTemplate(
                                solicitudAdopcion.NombreSolicitante,
                                animalNombre,
                                animalEspecie,
                                solicitudAdopcion.Id.ToString()
                            )
                        );
                    }
                }
                catch (Exception)
                {
                    // Prevenir caída del flujo si falla el servicio SMTP
                }

                return RedirectToAction(nameof(Index));
            }

            // Si hay algún error, volver a buscar el animal para volver a renderizar la vista correctamente
            var animalErr = await _context.AnimalesAdopcion.FindAsync(solicitudAdopcion.AnimalAdopcionId);
            ViewBag.Animal = animalErr;
            ViewData["AnimalAdopcionId"] = new SelectList(new List<AnimalAdopcion> { animalErr! }, "Id", "Nombre", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion.FindAsync(id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // POST: SolicitudesAdopcion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreSolicitante,Telefono,Email,Estado,FechaSolicitud,Activo,FechaCreacion,AnimalAdopcionId,Motivo")] SolicitudAdopcion solicitudAdopcion)
        {
            if (id != solicitudAdopcion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    solicitudAdopcion.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(solicitudAdopcion);
                    await _context.SaveChangesAsync();

                    // Si se aprueba la solicitud, marcar al animal como adoptado (no disponible)
                    if (solicitudAdopcion.Estado == "Aprobada")
                    {
                        var animal = await _context.AnimalesAdopcion.FindAsync(solicitudAdopcion.AnimalAdopcionId);
                        if (animal != null && animal.Disponible)
                        {
                            animal.Disponible = false;
                            animal.FechaActualizacion = DateTime.UtcNow;
                            _context.Update(animal);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudAdopcionExists(solicitudAdopcion.Id))
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
            ViewData["AnimalAdopcionId"] = new SelectList(_context.AnimalesAdopcion, "Id", "Especie", solicitudAdopcion.AnimalAdopcionId);
            return View(solicitudAdopcion);
        }

        // GET: SolicitudesAdopcion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudAdopcion = await _context.SolicitudesAdopcion
                .Include(s => s.AnimalAdopcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitudAdopcion == null)
            {
                return NotFound();
            }

            return View(solicitudAdopcion);
        }

        // POST: SolicitudesAdopcion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudAdopcion = await _context.SolicitudesAdopcion.FindAsync(id);
            if (solicitudAdopcion != null)
            {
                solicitudAdopcion.Activo = false;
                solicitudAdopcion.FechaEliminacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudAdopcionExists(int id)
        {
            return _context.SolicitudesAdopcion.Any(e => e.Id == id);
        }
    }
}
