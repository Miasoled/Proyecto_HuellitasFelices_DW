using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;

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
            var consultaQuery = _context.Consultas
                .AsNoTracking()
                .Include(c => c.Mascota)
                .Include(c => c.Veterinario)
                .Include(c => c.Venta)
                .Where(c => c.Activo)
                .OrderByDescending(c => c.FechaConsulta)
                .AsQueryable();

            // Filtrado por seguridad: si es cliente, sólo puede ver consultas de sus mascotas
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                if (dueno != null)
                {
                    consultaQuery = consultaQuery.Where(c => c.Mascota!.DuenoId == dueno.Id);
                }
                else
                {
                    consultaQuery = consultaQuery.Where(c => false);
                }
            }

            // Filtrado para doctor: ve pendientes y en revisión, NO completadas
            if (User.IsInRole("Doctor"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var doctorUser = await _context.Empleados.FirstOrDefaultAsync(e => e.Email == userEmail && e.Activo);
                if (doctorUser != null)
                {
                    consultaQuery = consultaQuery.Where(c => c.VeterinarioId == doctorUser.Id && c.Estado != "Completada");
                }
                else
                {
                    consultaQuery = consultaQuery.Where(c => false);
                }
            }

            // Filtrado para admin: ve solo completadas (las pendientes las ve el doctor)
            if (User.IsInRole("Administrador"))
            {
                consultaQuery = consultaQuery.Where(c => c.Estado == "Completada");
            }

            if (!string.IsNullOrEmpty(busqueda))
                consultaQuery = consultaQuery.Where(c => c.Motivo.Contains(busqueda));

            var totalRegistros = await consultaQuery.CountAsync();
            var consultas = await consultaQuery
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
            if (id == null) return NotFound();

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                    .ThenInclude(m => m!.Dueno)
                .Include(c => c.Veterinario)
                .Include(c => c.Tratamientos.Where(t => t.Activo))
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (consulta == null) return NotFound();

            // Restricción de seguridad para clientes
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                if (dueno == null || consulta.Mascota!.DuenoId != dueno.Id)
                    return Forbid();
            }

            // Restricción de seguridad para doctor: sólo sus consultas
            if (User.IsInRole("Doctor"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var doctorUser = await _context.Empleados.FirstOrDefaultAsync(e => e.Email == userEmail && e.Activo);
                if (doctorUser == null || consulta.VeterinarioId != doctorUser.Id)
                    return Forbid();
            }

            return View(consulta);
        }

        // GET: Consultas/Create
        public async Task<IActionResult> Create()
        {
            List<Mascota> mascotas;
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                mascotas = dueno != null 
                    ? await _context.Mascotas.Where(m => m.DuenoId == dueno.Id && m.Activo).ToListAsync()
                    : new List<Mascota>();
            }
            else
            {
                mascotas = await _context.Mascotas.Where(m => m.Activo).ToListAsync();
            }

            ViewData["MascotaId"] = new SelectList(mascotas, "Id", "Nombre");
            ViewData["VeterinarioId"] = new SelectList(
                await _context.Empleados.Where(e => e.Cargo == "Veterinario" && e.Activo).ToListAsync(),
                "Id", "Nombre");
            return View(new Consulta { FechaConsulta = DateTime.Now });
        }

        // POST: Consultas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Motivo,Sintomas,Diagnostico,Costo,FechaConsulta,Activo,MascotaId,Estado,VeterinarioId")] Consulta consulta)
        {
            ModelState.Remove("Mascota");

            // Validar que la mascota pertenezca al dueño si es cliente
            if (User.IsInRole("Cliente"))
            {
                // Forzar campos médicos a vacíos
                consulta.Costo = 0;
                consulta.Diagnostico = null;
                ModelState.Remove("Costo");
                ModelState.Remove("Diagnostico");

                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                var mascota = await _context.Mascotas.FindAsync(consulta.MascotaId);
                if (dueno == null || mascota == null || mascota.DuenoId != dueno.Id || !mascota.Activo)
                {
                    ModelState.AddModelError("MascotaId", "La mascota seleccionada es inválida.");
                }

                // Validar que el cliente seleccione un veterinario
                if (consulta.VeterinarioId == null || consulta.VeterinarioId == 0)
                {
                    ModelState.AddModelError("VeterinarioId", "Debe seleccionar un veterinario.");
                }
                else
                {
                    var vet = await _context.Empleados.FindAsync(consulta.VeterinarioId);
                    if (vet == null || !vet.Activo || vet.Cargo != "Veterinario")
                    {
                        ModelState.AddModelError("VeterinarioId", "El veterinario seleccionado es inválido.");
                    }
                }
            }
            else
            {
                var mascota = await _context.Mascotas.FindAsync(consulta.MascotaId);
                if (mascota == null || !mascota.Activo)
                {
                    ModelState.AddModelError("MascotaId", "La mascota seleccionada es inválida.");
                }
            }

            if (ModelState.IsValid)
            {
                consulta.FechaCreacion = DateTime.UtcNow;
                consulta.FechaActualizacion = DateTime.UtcNow;
                consulta.Activo = true;
                consulta.Estado = User.IsInRole("Cliente") ? "Pendiente" : (consulta.Costo > 0 ? "EnRevision" : "Pendiente");

                _context.Add(consulta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar el dropdown de mascotas si falla la validación
            List<Mascota> mascotas;
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                mascotas = dueno != null 
                    ? await _context.Mascotas.Where(m => m.DuenoId == dueno.Id && m.Activo).ToListAsync()
                    : new List<Mascota>();
            }
            else
            {
                mascotas = await _context.Mascotas.Where(m => m.Activo).ToListAsync();
            }

            ViewData["MascotaId"] = new SelectList(mascotas, "Id", "Nombre", consulta.MascotaId);
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
                .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (consulta == null)
            {
                return NotFound();
            }

            // Restricción de seguridad para doctor: sólo sus consultas
            if (User.IsInRole("Doctor"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var doctorUser = await _context.Empleados.FirstOrDefaultAsync(e => e.Email == userEmail && e.Activo);
                if (doctorUser == null || consulta.VeterinarioId != doctorUser.Id)
                {
                    return Forbid();
                }

                var allPets = await _context.Mascotas.Where(m => m.Activo).ToListAsync();
                ViewData["MascotaId"] = new SelectList(allPets, "Id", "Nombre", consulta.MascotaId);
                ViewData["VeterinarioId"] = new SelectList(
                    await _context.Empleados.Where(e => e.Cargo == "Veterinario" && e.Activo).ToListAsync(),
                    "Id", "Nombre", consulta.VeterinarioId);
                ViewData["ProductosDisponibles"] = await _context.Productos
                    .Where(p => p.Activo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventarios)
                    .ToListAsync();
                return View(consulta);
            }

            // Restricción de seguridad para clientes
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                if (dueno == null || consulta.Mascota!.DuenoId != dueno.Id)
                {
                    return Forbid();
                }

                var clientPets = await _context.Mascotas.Where(m => m.DuenoId == dueno.Id && m.Activo).ToListAsync();
                ViewData["MascotaId"] = new SelectList(clientPets, "Id", "Nombre", consulta.MascotaId);
            }
            else
            {
                var allPets = await _context.Mascotas.Where(m => m.Activo).ToListAsync();
                ViewData["MascotaId"] = new SelectList(allPets, "Id", "Nombre", consulta.MascotaId);
            }

            return View(consulta);
        }

        // POST: Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Motivo,Sintomas,Diagnostico,Costo,FechaConsulta,Activo,FechaCreacion,MascotaId,Estado,VeterinarioId")] Consulta consulta,
            string[]? medProductoId, int[]? medCantidad, string[]? medDosis, string[]? medIndicaciones)
        {
            if (id != consulta.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Mascota");

            // Buscar consulta existente para verificar seguridad y preservar campos
            var consultaExistente = await _context.Consultas
                .AsNoTracking()
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (consultaExistente == null)
            {
                return NotFound();
            }

            // Restricción de seguridad para doctor: sólo sus consultas
            if (User.IsInRole("Doctor"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var doctorUser = await _context.Empleados.FirstOrDefaultAsync(e => e.Email == userEmail && e.Activo);
                if (doctorUser == null || consultaExistente.VeterinarioId != doctorUser.Id)
                {
                    return Forbid();
                }

                consulta.Motivo = consultaExistente.Motivo;
                consulta.MascotaId = consultaExistente.MascotaId;
                ModelState.Remove("Motivo");
                ModelState.Remove("MascotaId");
            }

            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                if (dueno == null || consultaExistente.Mascota!.DuenoId != dueno.Id)
                {
                    return Forbid();
                }

                consulta.Costo = consultaExistente.Costo;
                consulta.Diagnostico = consultaExistente.Diagnostico;
                ModelState.Remove("Costo");
                ModelState.Remove("Diagnostico");

                var mascota = await _context.Mascotas.FindAsync(consulta.MascotaId);
                if (mascota == null || mascota.DuenoId != dueno.Id || !mascota.Activo)
                {
                    ModelState.AddModelError("MascotaId", "La mascota seleccionada es inválida.");
                }
            }
            else
            {
                var mascota = await _context.Mascotas.FindAsync(consulta.MascotaId);
                if (mascota == null || !mascota.Activo)
                {
                    ModelState.AddModelError("MascotaId", "La mascota seleccionada es inválida.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    consulta.FechaActualizacion = DateTime.UtcNow;

                    // El doctor pone EnRevision al guardar. Solo se marca Completada al pagar.
                    if (consulta.Costo > 0 && consulta.Estado != "Completada")
                        consulta.Estado = "EnRevision";

                    _context.Update(consulta);

                    // Guardar medicamentos solo si el doctor los envió
                    if (User.IsInRole("Doctor") && medProductoId != null)
                    {
                        // Eliminar medicamentos anteriores
                        var medsExistentes = await _context.ConsultaMedicamentos
                            .Where(cm => cm.ConsultaId == id).ToListAsync();
                        _context.ConsultaMedicamentos.RemoveRange(medsExistentes);

                        for (int i = 0; i < medProductoId.Length; i++)
                        {
                            if (int.TryParse(medProductoId[i], out int prodId) && prodId > 0)
                            {
                                var producto = await _context.Productos.FindAsync(prodId);
                                if (producto == null || !producto.Activo) continue;

                                var cantidad = (medCantidad != null && i < medCantidad.Length && medCantidad[i] > 0) ? medCantidad[i] : 1;
                                var dosis = (medDosis != null && i < medDosis.Length) ? medDosis[i] : null;
                                var indicaciones = (medIndicaciones != null && i < medIndicaciones.Length) ? medIndicaciones[i] : null;

                                _context.ConsultaMedicamentos.Add(new ConsultaMedicamento
                                {
                                    ConsultaId = id,
                                    ProductoId = prodId,
                                    Cantidad = cantidad,
                                    PrecioUnitario = producto.PrecioVenta,
                                    Dosis = dosis,
                                    Indicaciones = indicaciones
                                });
                            }
                        }
                    }

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

            // Recargar select lists
            List<Mascota> mascotas;
            if (User.IsInRole("Cliente"))
            {
                string userEmail = User.Identity?.Name ?? string.Empty;
                var dueno = await _context.Duenos.FirstOrDefaultAsync(d => d.Email == userEmail && d.Activo);
                mascotas = dueno != null 
                    ? await _context.Mascotas.Where(m => m.DuenoId == dueno.Id && m.Activo).ToListAsync()
                    : new List<Mascota>();
            }
            else
            {
                mascotas = await _context.Mascotas.Where(m => m.Activo).ToListAsync();
            }

            ViewData["MascotaId"] = new SelectList(mascotas, "Id", "Nombre", consulta.MascotaId);
            ViewData["VeterinarioId"] = new SelectList(
                await _context.Empleados.Where(e => e.Cargo == "Veterinario" && e.Activo).ToListAsync(),
                "Id", "Nombre", consulta.VeterinarioId);
            if (User.IsInRole("Doctor"))
            {
                ViewData["ProductosDisponibles"] = await _context.Productos
                    .Where(p => p.Activo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventarios)
                    .ToListAsync();
            }
            return View(consulta);
        }

        // GET: Consultas/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consulta = await _context.Consultas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (consulta == null)
            {
                return NotFound();
            }

            return View(consulta);
        }

        // POST: Consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
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
