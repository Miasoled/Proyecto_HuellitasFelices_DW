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
    public class DuenosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private const int TamanioPagina = 20;

        public DuenosController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
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
                consulta = consulta.Where(d => EF.Functions.ILike(d.Nombre, $"%{busqueda}%") || EF.Functions.ILike(d.Email!, $"%{busqueda}%"));

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
                await _auditService.LogAsync("Creacion", "Dueno", dueno.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorNuevo: dueno.Nombre);
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
                    var anterior = await _context.Duenos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    _context.Update(dueno);
                    await _context.SaveChangesAsync();
                    await _auditService.LogAsync("Edicion", "Dueno", dueno.Id,
                        usuarioEmail: User.Identity?.Name,
                        direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                        valorAnterior: anterior?.Nombre,
                        valorNuevo: dueno.Nombre);
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

        // POST: Duenos/Delete/5 — Solo Administrador
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno != null)
            {
                dueno.Activo = false;
                dueno.FechaEliminacion = DateTime.UtcNow;
                dueno.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("EliminacionLogica", "Dueno", dueno.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorAnterior: "Registro activo",
                    valorNuevo: "Registro eliminado lógicamente");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DuenoExists(int id)
        {
            return _context.Duenos.Any(e => e.Id == id);
        }
    }
}
