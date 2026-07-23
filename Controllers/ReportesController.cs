using HuellitasFelices.Data;
using HuellitasFelices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Controllers
{
    [Authorize(Roles = "Administrador,Supervisor")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Reportes
        public async Task<IActionResult> Index()
        {
            var vm = new ReporteViewModel();

            vm.TotalIngresosConsultas = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .SumAsync(c => c.Costo);

            vm.PromedioCostoConsulta = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .AverageAsync(c => c.Costo);

            vm.TotalMascotas    = await _context.Mascotas.CountAsync();
            vm.TotalDuenos      = await _context.Duenos.CountAsync();
            vm.TotalConsultas   = await _context.Consultas.CountAsync();
            vm.MascotasActivas  = await _context.Mascotas.CountAsync(m => m.Activo);
            vm.MascotasInactivas = await _context.Mascotas.CountAsync(m => !m.Activo);

            vm.ConsultasPorMes = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo && c.FechaConsulta >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(c => new { c.FechaConsulta.Year, c.FechaConsulta.Month })
                .Select(g => new ResumenMensual
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalConsultas = g.Count(),
                    TotalIngresos = g.Sum(c => c.Costo)
                })
                .OrderBy(r => r.Anio).ThenBy(r => r.Mes)
                .ToListAsync();

            vm.Top10Motivos = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .GroupBy(c => c.Motivo)
                .Select(g => new ResumenMotivo
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    TotalIngresos = g.Sum(c => c.Costo)
                })
                .OrderByDescending(r => r.Cantidad)
                .Take(10)
                .ToListAsync();

            vm.Top10DuenosConMascotas = await _context.Duenos
                .AsNoTracking()
                .Where(d => d.Activo)
                .Select(d => new ResumenDueno
                {
                    Nombre = d.Nombre,
                    Email = d.Email,
                    TotalMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .Where(r => r.TotalMascotas > 0)
                .OrderByDescending(r => r.TotalMascotas)
                .Take(10)
                .ToListAsync();

            vm.EmpleadosPorCargo = await _context.Empleados
                .AsNoTracking()
                .Where(e => e.Activo)
                .GroupBy(e => e.Cargo)
                .Select(g => new ResumenCargo
                {
                    Cargo = g.Key,
                    Cantidad = g.Count(),
                    SalarioPromedio = g.Average(e => e.Salario)
                })
                .OrderByDescending(r => r.Cantidad)
                .ToListAsync();

            return View(vm);
        }

        // ── Reportes separados ──────────────────────────────────────────────

        public async Task<IActionResult> ReporteConsultas(DateTime? desde, DateTime? hasta)
        {
            var d = desde ?? DateTime.UtcNow.AddMonths(-6);
            var h = hasta ?? DateTime.UtcNow;
            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");

            var consultas = await _context.Consultas
                .AsNoTracking()
                .Include(c => c.Mascota).ThenInclude(m => m!.Dueno)
                .Where(c => c.Activo && c.FechaConsulta >= d && c.FechaConsulta <= h)
                .OrderByDescending(c => c.FechaConsulta)
                .ToListAsync();

            ViewBag.TotalCosto = consultas.Sum(c => c.Costo);
            ViewBag.TotalConsultas = consultas.Count;
            return View(consultas);
        }

        public async Task<IActionResult> ReporteMascotas()
        {
            var mascotas = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Dueno)
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.Total = mascotas.Count;
            return View(mascotas);
        }

        public async Task<IActionResult> ReporteDuenos()
        {
            var duenos = await _context.Duenos
                .AsNoTracking()
                .Where(d => d.Activo)
                .Select(d => new ResumenDueno
                {
                    Nombre = d.Nombre,
                    Email = d.Email,
                    TotalMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .OrderByDescending(r => r.TotalMascotas)
                .ToListAsync();

            ViewBag.Total = duenos.Count;
            return View(duenos);
        }

        public async Task<IActionResult> ReporteEmpleados()
        {
            var empleados = await _context.Empleados
                .AsNoTracking()
                .Where(e => e.Activo)
                .OrderBy(e => e.Cargo).ThenBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.Total = empleados.Count;
            ViewBag.SalarioTotal = empleados.Sum(e => e.Salario);
            return View(empleados);
        }

        public async Task<IActionResult> ReporteAdopciones()
        {
            var solicitudes = await _context.SolicitudesAdopcion
                .AsNoTracking()
                .Include(s => s.AnimalAdopcion)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            ViewBag.Total = solicitudes.Count;
            ViewBag.Aprobadas = solicitudes.Count(s => s.Estado == "Aprobada");
            ViewBag.Pendientes = solicitudes.Count(s => s.Estado == "Pendiente");
            return View(solicitudes);
        }

        public async Task<IActionResult> ReporteServicios()
        {
            var motivos = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .GroupBy(c => c.Motivo)
                .Select(g => new ResumenMotivo
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    TotalIngresos = g.Sum(c => c.Costo)
                })
                .OrderByDescending(r => r.Cantidad)
                .ToListAsync();

            return View(motivos);
        }

        public async Task<IActionResult> ReporteInventario()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.TotalProductos = productos.Count;
            ViewBag.StockTotal = productos.SelectMany(p => p.Inventarios).Sum(i => i.StockActual);
            return View(productos);
        }

        public async Task<IActionResult> ReporteAuditoria(int pagina = 1, string? accion = null, string? entidad = null)
        {
            const int tamPagina = 50;
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);
            if (!string.IsNullOrEmpty(entidad))
                query = query.Where(a => a.Entidad == entidad);

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((pagina - 1) * tamPagina)
                .Take(tamPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamPagina);
            ViewBag.TotalRegistros = total;
            ViewBag.Accion = accion;
            ViewBag.Entidad = entidad;
            ViewBag.Acciones = await _context.AuditLogs.Select(a => a.Accion).Distinct().ToListAsync();
            ViewBag.Entidades = await _context.AuditLogs.Select(a => a.Entidad).Distinct().ToListAsync();

            return View(logs);
        }
    }
}
