using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class SolicitudesAdopcionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private const int TamanioPagina = 20;

        public SolicitudesAdopcionController(AppDbContext context, IEmailService emailService, IAuditService auditService)
        {
            _context = context;
            _emailService = emailService;
            _auditService = auditService;
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
                consulta = consulta.Where(s => EF.Functions.ILike(s.NombreSolicitante, $"%{busqueda}%") || EF.Functions.ILike(s.Estado, $"%{busqueda}%"));

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

                        await _emailService.EnviarAdopcionAsync(
                            solicitudAdopcion.Email,
                            solicitudAdopcion.NombreSolicitante,
                            animalNombre,
                            animalEspecie,
                            solicitudAdopcion.Id.ToString()
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

                     if (solicitudAdopcion.Estado == "Aprobada")
                    {
                        var animal = await _context.AnimalesAdopcion.FindAsync(solicitudAdopcion.AnimalAdopcionId);
                        if (animal != null)
                        {
                            if (animal.Disponible)
                            {
                                animal.Disponible = false;
                                animal.FechaActualizacion = DateTime.UtcNow;
                                _context.Update(animal);
                                await _context.SaveChangesAsync();
                            }

                            var dueno = await _context.Duenos.FirstOrDefaultAsync(d =>
                                d.Email != null &&
                                d.Email.ToLower() == solicitudAdopcion.Email!.ToLower() &&
                                d.Activo);

                            if (dueno == null && !string.IsNullOrEmpty(solicitudAdopcion.Email))
                            {
                                dueno = await _context.Duenos.FirstOrDefaultAsync(d =>
                                    d.Email != null &&
                                    d.Email.ToLower() == solicitudAdopcion.Email.ToLower());
                            }

                            if (dueno != null)
                            {
                                var yaExiste = await _context.Mascotas.AnyAsync(m =>
                                    m.Nombre.ToLower() == animal.Nombre.ToLower() &&
                                    m.DuenoId == dueno.Id &&
                                    m.Activo);

                                if (!yaExiste)
                                {
                                    var nuevaMascota = new Mascota
                                    {
                                        Nombre = animal.Nombre,
                                        Especie = animal.Especie,
                                        Raza = animal.Raza ?? "Mestizo",
                                        Sexo = "Macho",
                                        FechaNacimiento = DateTime.UtcNow.AddYears(-animal.EdadAproximada),
                                        Peso = 5.0m,
                                        DuenoId = dueno.Id,
                                        Activo = true,
                                        FechaCreacion = DateTime.UtcNow,
                                        FechaActualizacion = DateTime.UtcNow
                                    };
                                    _context.Mascotas.Add(nuevaMascota);
                                    await _context.SaveChangesAsync();
                                    TempData["Mensaje"] = $"Mascota '{animal.Nombre}' registrada exitosamente para el dueño {dueno.Nombre}.";
                                }
                                else
                                {
                                    TempData["Error"] = $"La mascota '{animal.Nombre}' ya esta registrada para este dueño.";
                                }
                            }
                            else
                            {
                                TempData["Error"] = $"No se encontro un dueño registrado con el email '{solicitudAdopcion.Email}'. La mascota no fue creada.";
                            }
                        }
                        else
                        {
                            TempData["Error"] = "No se encontro el animal asociado a esta solicitud.";
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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudAdopcion = await _context.SolicitudesAdopcion.FindAsync(id);
            if (solicitudAdopcion != null)
            {
                solicitudAdopcion.Activo = false;
                solicitudAdopcion.FechaEliminacion = DateTime.UtcNow;
                solicitudAdopcion.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("EliminacionLogica", "SolicitudAdopcion", solicitudAdopcion.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorAnterior: "Registro activo",
                    valorNuevo: "Registro eliminado lógicamente");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudAdopcionExists(int id)
        {
            return _context.SolicitudesAdopcion.Any(e => e.Id == id);
        }
    }
}
