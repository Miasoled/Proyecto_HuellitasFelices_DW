using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Controllers
{
[Authorize(Roles = "Administrador,Supervisor,Auditor")]
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

        public async Task<IActionResult> ReportePagos(
            int pagina = 1, string? busqueda = null, string? estado = null,
            string? proveedor = null, DateTime? desde = null, DateTime? hasta = null)
        {
            const int tamPagina = 20;

            var query = _context.Pagos
                .AsNoTracking()
                .Include(p => p.Venta).ThenInclude(v => v!.Consulta).ThenInclude(c => c!.Mascota)
                .Include(p => p.Dueno)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(p => EF.Functions.ILike(p.NumeroPago, $"%{busqueda}%") ||
                    (p.Dueno != null && EF.Functions.ILike(p.Dueno.Nombre, $"%{busqueda}%")));

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrWhiteSpace(proveedor))
                query = query.Where(p => p.ProveedorPago == proveedor);

            if (desde.HasValue)
                query = query.Where(p => p.FechaCreacion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(p => p.FechaCreacion <= hasta.Value);

            var total = await query.CountAsync();
            var pagos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * tamPagina)
                .Take(tamPagina)
                .ToListAsync();

            var resumen = await _context.Pagos
                .AsNoTracking()
                .GroupBy(p => new { p.ProveedorPago, p.Estado })
                .Select(g => new
                {
                    Proveedor = g.Key.ProveedorPago,
                    Estado = g.Key.Estado,
                    Cantidad = g.Count(),
                    MontoTotal = g.Sum(p => p.Monto)
                })
                .OrderBy(x => x.Proveedor).ThenBy(x => x.Estado)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tamPagina);
            ViewBag.TotalRegistros = total;
            ViewBag.Busqueda = busqueda;
            ViewBag.Estado = estado;
            ViewBag.Proveedor = proveedor;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Resumen = resumen;

            return View(pagos);
        }

        // ── Reportes adicionales para el tercer parcial ──────────────────

        public async Task<IActionResult> ReporteVentas(DateTime? desde, DateTime? hasta, string? sucursal)
        {
            var d = desde ?? DateTime.UtcNow.AddMonths(-6);
            var h = hasta ?? DateTime.UtcNow;

            var query = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Dueno)
                .Include(v => v.Sucursal)
                .Where(v => v.Activo && v.FechaVenta >= d && v.FechaVenta <= h)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sucursal))
                query = query.Where(v => v.Sucursal != null && v.Sucursal.Nombre == sucursal);

            var ventas = await query.OrderByDescending(v => v.FechaVenta).ToListAsync();

            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");
            ViewBag.TotalVentas = ventas.Count;
            ViewBag.MontoTotal = ventas.Sum(v => v.Total);
            ViewBag.Sucursales = await _context.Sucursales.Where(s => s.Activo).Select(s => s.Nombre).ToListAsync();
            ViewBag.SucursalSeleccionada = sucursal;

            return View(ventas);
        }

        public async Task<IActionResult> ReporteProductosMasVendidos()
        {
            var productos = await _context.DetallesVenta
                .AsNoTracking()
                .Include(dv => dv.Producto).ThenInclude(p => p!.Categoria)
                .Where(dv => dv.Venta != null && dv.Venta.Activo)
                .GroupBy(dv => new { dv.ProductoId, dv.Producto!.Nombre, Categoria = dv.Producto.Categoria!.Nombre })
                .Select(g => new
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    TotalVendido = g.Sum(dv => dv.Cantidad),
                    IngresosTotales = g.Sum(dv => dv.Cantidad * dv.PrecioUnitario)
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(50)
                .ToListAsync();

            ViewBag.Total = productos.Count;
            return View(productos);
        }

        public async Task<IActionResult> ReporteBajoInventario()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    Categoria = p.Categoria!.Nombre,
                    StockMinimo = p.StockMinimo,
                    StockActual = p.Inventarios.Sum(i => i.StockActual),
                    p.PrecioVenta
                })
                .Where(x => x.StockActual <= x.StockMinimo)
                .OrderBy(x => x.StockActual)
                .ToListAsync();

            ViewBag.Total = productos.Count;
            return View(productos);
        }

        public async Task<IActionResult> ReporteClientesCompras()
        {
            var clientes = await _context.Duenos
                .AsNoTracking()
                .Where(d => d.Activo)
                .Select(d => new
                {
                    d.Nombre,
                    d.Email,
                    TotalConsultas = d.Mascotas.SelectMany(m => m.Consultas).Count(c => c.Activo),
                    TotalMascotas = d.Mascotas.Count(m => m.Activo),
                    MontoTotal = d.Mascotas.SelectMany(m => m.Consultas).Where(c => c.Activo).Sum(c => c.Costo)
                })
                .Where(x => x.TotalConsultas > 0)
                .OrderByDescending(x => x.MontoTotal)
                .Take(50)
                .ToListAsync();

            ViewBag.Total = clientes.Count;
            return View(clientes);
        }

        public async Task<IActionResult> ReporteMFA()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Forbid();

            var userManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
            var users = await userManager.Users.ToListAsync();

            var resultado = new List<object>();
            int conMFA = 0;
            foreach (var user in users)
            {
                var mfaEnabled = await userManager.GetTwoFactorEnabledAsync(user);
                var roles = await userManager.GetRolesAsync(user);
                if (mfaEnabled) conMFA++;
                resultado.Add(new
                {
                    user.Email,
                    MFAHabilitado = mfaEnabled,
                    Rol = roles.FirstOrDefault() ?? "Sin rol"
                });
            }

            ViewBag.Total = resultado.Count;
            ViewBag.ConMFA = conMFA;
            ViewBag.SinMFA = resultado.Count - conMFA;

            return View(resultado);
        }

        public async Task<IActionResult> ReporteAccesosFallidos(DateTime? desde, DateTime? hasta)
        {
            var d = desde ?? DateTime.UtcNow.AddMonths(-1);
            var h = hasta ?? DateTime.UtcNow;

            var query = _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Accion == "LoginFallido" || a.Accion == "CuentaBloqueada")
                .Where(a => a.FechaCreacion >= d && a.FechaCreacion <= h)
                .AsQueryable();

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Take(200)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");

            return View(logs);
        }
    }
}
