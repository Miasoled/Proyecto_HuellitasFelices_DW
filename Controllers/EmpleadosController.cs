using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;

namespace HuellitasFelices.Controllers
{
[Authorize(Roles = "Administrador,Supervisor")]
public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private const int TamanioPagina = 20;

        public EmpleadosController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Empleados
        public async Task<IActionResult> Index(int pagina = 1, string? busqueda = null)
        {
            var consulta = _context.Empleados
                .AsNoTracking()
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                consulta = consulta.Where(e => EF.Functions.ILike(e.Nombre, $"%{busqueda}%") || EF.Functions.ILike(e.Cargo, $"%{busqueda}%"));

            var totalRegistros = await consulta.CountAsync();
            var empleados = await consulta
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

            return View(empleados);
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Empleados/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Cargo,Email,Telefono,Salario,Activo,FechaCreacion,SucursalId")] Empleado empleado)
        {
            if (empleado.Cargo == "Veterinario")
            {
                var totalVets = await _context.Empleados.CountAsync(e => e.Cargo == "Veterinario" && e.Activo);
                if (totalVets >= 3)
                {
                    ModelState.AddModelError("Cargo", "Solo se permiten 3 doctores veterinarios en el sistema.");
                    return View(empleado);
                }
            }

            if (ModelState.IsValid)
            {
                empleado.FechaCreacion = DateTime.UtcNow;
                empleado.Activo = true;
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Creacion", "Empleado", empleado.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorNuevo: $"{empleado.Nombre} - {empleado.Cargo}");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", empleado.SucursalId);
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", empleado.SucursalId);
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cargo,Email,Telefono,Salario,Activo,FechaCreacion,SucursalId")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var anterior = await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    await _auditService.LogAsync("Edicion", "Empleado", empleado.Id,
                        usuarioEmail: User.Identity?.Name,
                        direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                        valorAnterior: anterior?.Nombre,
                        valorNuevo: empleado.Nombre);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id))
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
            ViewBag.SucursalId = new SelectList(await _context.Sucursales.Where(s => s.Activo).ToListAsync(), "Id", "Nombre", empleado.SucursalId);
            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                empleado.Activo = false;
                empleado.FechaEliminacion = DateTime.UtcNow;
                empleado.EliminadoPor = User.Identity?.Name;
                await _context.SaveChangesAsync();
                await _auditService.LogAsync("EliminacionLogica", "Empleado", empleado.Id,
                    usuarioEmail: User.Identity?.Name,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    valorAnterior: "Registro activo",
                    valorNuevo: "Registro eliminado lógicamente");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}
