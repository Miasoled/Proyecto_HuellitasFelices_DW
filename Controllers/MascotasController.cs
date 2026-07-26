using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
    [Authorize]
    public class MascotasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        private const int TamanioPagina = 20;

        public MascotasController(AppDbContext context, UserManager<IdentityUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: Mascotas
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Dueno)
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(m => EF.Functions.ILike(m.Nombre, $"%{busqueda}%") || EF.Functions.ILike(m.Especie, $"%{busqueda}%"));

            var totalRegistros = await consulta.CountAsync();
            var mascotas = await consulta
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

            return View(mascotas);
        }

        // GET: Mascotas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas
                .Include(m => m.Dueno)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        // GET: Mascotas/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var dueno = _context.Duenos.FirstOrDefault(d => d.Email == user!.Email && d.Activo);

            if (dueno == null)
            {
                TempData["Error"] = "No se encontro tu perfil de dueño. Contacta al administrador.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Dueno = dueno;
            return View();
        }

        // POST: Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Especie,Raza,Sexo,FechaNacimiento,Peso")] Mascota mascota)
        {
            var user = await _userManager.GetUserAsync(User);
            var dueno = _context.Duenos.FirstOrDefault(d => d.Email == user!.Email && d.Activo);

            if (dueno == null)
            {
                TempData["Error"] = "No se encontro tu perfil de dueño.";
                return RedirectToAction("Index", "Home");
            }

            mascota.DuenoId = dueno.Id;
            mascota.Activo = true;
            mascota.FechaCreacion = DateTime.UtcNow;
            mascota.FechaActualizacion = DateTime.UtcNow;

            ModelState.Remove("DuenoId");

            if (ModelState.IsValid)
            {
                _context.Add(mascota);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Creacion", "Mascota", mascota.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorNuevo: mascota.Nombre);
                TempData["Mensaje"] = $"Mascota '{mascota.Nombre}' registrada exitosamente.";
                return RedirectToAction("MiPanel", "Account");
            }
            ViewBag.Dueno = dueno;
            return View(mascota);
        }

        // GET: Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null)
            {
                return NotFound();
            }
            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre", mascota.DuenoId);
            return View(mascota);
        }

        // POST: Mascotas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Especie,Raza,Sexo,FechaNacimiento,Peso,Activo,FechaCreacion,DuenoId")] Mascota mascota)
        {
            if (id != mascota.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var anterior = await _context.Mascotas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                    _context.Update(mascota);
                    await _context.SaveChangesAsync();
                    await _auditService.LogAsync("Edicion", "Mascota", mascota.Id,
                        usuarioEmail: User.Identity?.Name,
                        direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                        valorAnterior: anterior?.Nombre,
                        valorNuevo: mascota.Nombre);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MascotaExists(mascota.Id))
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
            ViewData["DuenoId"] = new SelectList(_context.Duenos, "Id", "Nombre", mascota.DuenoId);
            return View(mascota);
        }

        // GET: Mascotas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas
                .Include(m => m.Dueno)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        // POST: Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota != null)
            {
                mascota.Activo = false;
                mascota.FechaEliminacion = DateTime.UtcNow;
                mascota.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("EliminacionLogica", "Mascota", mascota.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorAnterior: "Registro activo",
                    valorNuevo: "Registro eliminado lógicamente");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MascotaExists(int id)
        {
            return _context.Mascotas.Any(e => e.Id == id);
        }
    }
}
