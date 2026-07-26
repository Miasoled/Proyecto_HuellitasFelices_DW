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
        private const int TamanioPagina = 20;
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        private void ConfigurarPaginacion(int pagina, int totalRegistros)
        {
            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanioPagina),
                TotalRegistros = totalRegistros,
                TamanioPagina = TamanioPagina
            };
        }

        private static int PaginaValida(int pagina, int totalRegistros)
        {
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)TamanioPagina));
            return Math.Clamp(pagina, 1, totalPaginas);
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

        public async Task<IActionResult> ReporteConsultas(DateTime? desde, DateTime? hasta, int pagina = 1)
        {
            var d = desde ?? DateTime.UtcNow.AddMonths(-6);
            var h = hasta ?? DateTime.UtcNow;
            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");

            var query = _context.Consultas
                .AsNoTracking()
                .Include(c => c.Mascota).ThenInclude(m => m!.Dueno)
                .Where(c => c.Activo && c.FechaConsulta >= d && c.FechaConsulta <= h)
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var consultas = await query.OrderByDescending(c => c.FechaConsulta)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.TotalCosto = await query.SumAsync(c => (decimal?)c.Costo) ?? 0;
            ViewBag.TotalConsultas = total;
            ConfigurarPaginacion(pagina, total);
            return View(consultas);
        }

        public async Task<IActionResult> ReporteMascotas(int pagina = 1)
        {
            var query = _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Dueno)
                .Where(m => m.Activo)
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var mascotas = await query.OrderBy(m => m.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ConfigurarPaginacion(pagina, total);
            return View(mascotas);
        }

        public async Task<IActionResult> ReporteDuenos(int pagina = 1)
        {
            var query = _context.Duenos
                .AsNoTracking()
                .Where(d => d.Activo)
                .Select(d => new ResumenDueno
                {
                    Nombre = d.Nombre,
                    Email = d.Email,
                    TotalMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var duenos = await query.OrderByDescending(r => r.TotalMascotas)
                .ThenBy(r => r.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ConfigurarPaginacion(pagina, total);
            return View(duenos);
        }

        public async Task<IActionResult> ReporteEmpleados(int pagina = 1)
        {
            var query = _context.Empleados
                .AsNoTracking()
                .Where(e => e.Activo)
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var empleados = await query.OrderBy(e => e.Cargo).ThenBy(e => e.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.SalarioTotal = await query.SumAsync(e => (decimal?)e.Salario) ?? 0;
            ConfigurarPaginacion(pagina, total);
            return View(empleados);
        }

        public async Task<IActionResult> ReporteAdopciones(int pagina = 1)
        {
            var query = _context.SolicitudesAdopcion
                .AsNoTracking()
                .Include(s => s.AnimalAdopcion)
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var solicitudes = await query.OrderByDescending(s => s.FechaSolicitud)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Aprobadas = await query.CountAsync(s => s.Estado == "Aprobada");
            ViewBag.Pendientes = await query.CountAsync(s => s.Estado == "Pendiente");
            ConfigurarPaginacion(pagina, total);
            return View(solicitudes);
        }

        public async Task<IActionResult> ReporteServicios(int pagina = 1)
        {
            var query = _context.Consultas
                .AsNoTracking()
                .Where(c => c.Activo)
                .GroupBy(c => c.Motivo)
                .Select(g => new ResumenMotivo
                {
                    Motivo = g.Key,
                    Cantidad = g.Count(),
                    TotalIngresos = g.Sum(c => c.Costo)
                })
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var motivos = await query.OrderByDescending(r => r.Cantidad)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ConfigurarPaginacion(pagina, total);
            return View(motivos);
        }

        public async Task<IActionResult> ReporteInventario(int pagina = 1)
        {
            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var productos = await query.OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.TotalProductos = total;
            ViewBag.StockTotal = await _context.Inventarios
                .Where(i => i.Producto != null && i.Producto.Activo)
                .SumAsync(i => (int?)i.StockActual) ?? 0;
            ConfigurarPaginacion(pagina, total);
            return View(productos);
        }

        public async Task<IActionResult> ReporteAuditoria(int pagina = 1, string? accion = null, string? entidad = null)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);
            if (!string.IsNullOrEmpty(entidad))
                query = query.Where(a => a.Entidad == entidad);

            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var logs = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)TamanioPagina);
            ViewBag.TotalRegistros = total;
            ViewBag.Accion = accion;
            ViewBag.Entidad = entidad;
            ViewBag.Acciones = await _context.AuditLogs.Select(a => a.Accion).Distinct().ToListAsync();
            ViewBag.Entidades = await _context.AuditLogs.Select(a => a.Entidad).Distinct().ToListAsync();
            ConfigurarPaginacion(pagina, total);

            return View(logs);
        }

        public async Task<IActionResult> ReportePagos(
            int pagina = 1, string? busqueda = null, string? estado = null,
            string? proveedor = null, DateTime? desde = null, DateTime? hasta = null)
        {
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
            pagina = PaginaValida(pagina, total);
            var pagos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * TamanioPagina)
                .Take(TamanioPagina)
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
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)TamanioPagina);
            ViewBag.TotalRegistros = total;
            ViewBag.Busqueda = busqueda;
            ViewBag.Estado = estado;
            ViewBag.Proveedor = proveedor;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Resumen = resumen;
            ConfigurarPaginacion(pagina, total);

            return View(pagos);
        }

        // ── Reportes adicionales para el tercer parcial ──────────────────

        public async Task<IActionResult> ReporteVentas(DateTime? desde, DateTime? hasta, string? sucursal, int pagina = 1)
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

            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var montoTotal = await query.SumAsync(v => (decimal?)v.Total) ?? 0;
            var ventas = await query.OrderByDescending(v => v.FechaVenta)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina).ToListAsync();

            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");
            ViewBag.TotalVentas = total;
            ViewBag.MontoTotal = montoTotal;
            ViewBag.Sucursales = await _context.Sucursales.Where(s => s.Activo).Select(s => s.Nombre).ToListAsync();
            ViewBag.SucursalSeleccionada = sucursal;
            ConfigurarPaginacion(pagina, total);

            return View(ventas);
        }

        public async Task<IActionResult> ReporteProductosMasVendidos(int pagina = 1)
        {
            var query = _context.DetallesVenta
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
                .AsQueryable();
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var productos = await query.OrderByDescending(x => x.TotalVendido)
                .ThenBy(x => x.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ConfigurarPaginacion(pagina, total);
            return View(productos);
        }

        public async Task<IActionResult> ReporteBajoInventario(int pagina = 1)
        {
            var query = _context.Productos
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
                .Where(x => x.StockActual <= x.StockMinimo);
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var productos = await query.OrderBy(x => x.StockActual).ThenBy(x => x.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ConfigurarPaginacion(pagina, total);
            return View(productos);
        }

        public async Task<IActionResult> ReporteClientesCompras(int pagina = 1)
        {
            var query = _context.Duenos
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
                .Where(x => x.TotalConsultas > 0);
            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var clientes = await query.OrderByDescending(x => x.MontoTotal)
                .ThenBy(x => x.Nombre)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ConfigurarPaginacion(pagina, total);
            return View(clientes);
        }

        public async Task<IActionResult> ReporteMFA(int pagina = 1)
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
            var total = resultado.Count;
            pagina = PaginaValida(pagina, total);
            resultado = resultado.Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina).ToList();
            ConfigurarPaginacion(pagina, total);

            return View(resultado);
        }

        public async Task<IActionResult> ReporteAccesosFallidos(DateTime? desde, DateTime? hasta, int pagina = 1)
        {
            var d = desde ?? DateTime.UtcNow.AddMonths(-1);
            var h = hasta ?? DateTime.UtcNow;

            var query = _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Accion == "LoginFallido" || a.Accion == "CuentaBloqueada")
                .Where(a => a.FechaCreacion >= d && a.FechaCreacion <= h)
                .AsQueryable();

            var total = await query.CountAsync();
            pagina = PaginaValida(pagina, total);
            var logs = await query
                .OrderByDescending(a => a.FechaCreacion)
                .Skip((pagina - 1) * TamanioPagina).Take(TamanioPagina)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");
            ConfigurarPaginacion(pagina, total);

            return View(logs);
        }
    }
}
