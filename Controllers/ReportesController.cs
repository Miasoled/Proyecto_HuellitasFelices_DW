using HuellitasFelices.Data;
using HuellitasFelices.ViewModels;
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

            // ── TOTALES ───────────────────────────────────────────────────────

            // Sum: total de ingresos por pagos realizados
            vm.TotalIngresosPagos = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.Activo && p.Estado == "Pagado")
                .SumAsync(p => p.Monto);

            // Sum: total de ingresos por consultas
            vm.TotalIngresosConsultas = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .SumAsync(c => c.Costo);

            // Average: promedio de monto por pago
            vm.PromedioMontoPago = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.Activo)
                .AverageAsync(p => p.Monto);

            // Average: promedio de costo por consulta
            vm.PromedioCostoConsulta = await _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .AverageAsync(c => c.Costo);

            // ── COUNT ─────────────────────────────────────────────────────────

            vm.TotalMascotas    = await _context.Mascotas.CountAsync();
            vm.TotalDuenos      = await _context.Duenos.CountAsync();
            vm.TotalConsultas   = await _context.Consultas.CountAsync();
            vm.TotalPagos       = await _context.Pagos.CountAsync();
            vm.PagosActivos     = await _context.Pagos.CountAsync(p => p.Activo);
            vm.PagosInactivos   = await _context.Pagos.CountAsync(p => !p.Activo);
            vm.MascotasActivas  = await _context.Mascotas.CountAsync(m => m.Activo);
            vm.MascotasInactivas = await _context.Mascotas.CountAsync(m => !m.Activo);

            // ── GROUPBY: Consultas por mes (últimos 12 meses) ─────────────────
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

            // ── GROUPBY + OrderByDescending: Top 10 motivos de consulta ───────
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

            // ── Top 10 dueños con más mascotas ────────────────────────────────
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

            // ── GROUPBY: Empleados por cargo con salario promedio ─────────────
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

            // ── GROUPBY: Pagos por método de pago ────────────────────────────
            vm.PagosPorMetodo = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.Activo)
                .GroupBy(p => p.MetodoPago)
                .Select(g => new ResumenMetodoPago
                {
                    MetodoPago = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(p => p.Monto)
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();

            return View(vm);
        }
    }
}
