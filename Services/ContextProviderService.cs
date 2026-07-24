using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;

namespace HuellitasFelices.Services;

public class ContextProviderService : IContextProviderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContextProviderService> _logger;

    private static readonly Dictionary<string[], string> KeywordQueries = new()
    {
        { new[] { "cliente", "clientes", "due\u00f1o", "due\u00f1os", "propietario" }, "clientes" },
        { new[] { "mascota", "mascotas", "perro", "perros", "gato", "gatos", "animal", "animales" }, "mascotas" },
        { new[] { "consulta", "consultas", "cita", "citas", "atencion" }, "consultas" },
        { new[] { "pendiente", "pendientes", "espera" }, "pendientes" },
        { new[] { "producto", "productos", "stock", "inventario", "existencia" }, "inventario" },
        { new[] { "venta", "ventas", "factura", "facturacion", "ingreso", "ingresos", "dinero", "cobro", "cobros", "vendido" }, "ventas" },
        { new[] { "doctor", "doctores", "veterinario", "veterinarios", "medico", "empleado", "empleados" }, "empleados" },
        { new[] { "adopcion", "adopciones", "solicitud", "solicitudes", "adoptar" }, "adopcion" },
        { new[] { "compra", "compras", "proveedor", "proveedores", "pedido" }, "compras" },
        { new[] { "tratamiento", "tratamientos", "medicamento", "medicamentos" }, "tratamientos" },
        { new[] { "resumen", "resumir", "general", "todo", "todo lo que", "dashboard", "estado" }, "resumen" }
    };

    public ContextProviderService(AppDbContext db, ILogger<ContextProviderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string> ObtenerContextoAsync(string preguntaUsuario)
    {
        var pregunta = preguntaUsuario.ToLowerInvariant();
        var categoriasDetectadas = new HashSet<string>();

        foreach (var (keywords, categoria) in KeywordQueries)
        {
            if (keywords.Any(k => pregunta.Contains(k)))
            {
                categoriasDetectadas.Add(categoria);
            }
        }

        if (categoriasDetectadas.Count == 0)
        {
            categoriasDetectadas.Add("resumen");
        }

        var contextos = new List<string>();

        foreach (var cat in categoriasDetectadas)
        {
            try
            {
                var contexto = await ObtenerContextoPorCategoriaAsync(cat, pregunta);
                if (!string.IsNullOrWhiteSpace(contexto))
                    contextos.Add(contexto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener contexto para categoría: {Categoria}", cat);
            }
        }

        return contextos.Count > 0
            ? string.Join(" | ", contextos)
            : "No hay datos disponibles en la base de datos en este momento.";
    }

    private async Task<string> ObtenerContextoPorCategoriaAsync(string categoria, string pregunta)
    {
        var hoy = DateTime.UtcNow.Date;

        return categoria switch
        {
            "clientes" => await ObtenerContextoClientesAsync(),
            "mascotas" => await ObtenerContextoMascotasAsync(),
            "consultas" => await ObtenerContextoConsultasAsync(pregunta, hoy),
            "pendientes" => await ObtenerContextoPendientesAsync(),
            "inventario" => await ObtenerContextoInventarioAsync(),
            "ventas" => await ObtenerContextoVentasAsync(pregunta, hoy),
            "empleados" => await ObtenerContextoEmpleadosAsync(),
            "adopcion" => await ObtenerContextoAdopcionAsync(),
            "compras" => await ObtenerContextoComprasAsync(),
            "tratamientos" => await ObtenerContextoTratamientosAsync(),
            "resumen" => await ObtenerContextoResumenAsync(hoy),
            _ => ""
        };
    }

    private async Task<string> ObtenerContextoClientesAsync()
    {
        var total = await _db.Duenos.CountAsync(d => d.Activo);
        var nuevosMes = await _db.Duenos.CountAsync(d => d.Activo && d.FechaCreacion.Month == DateTime.UtcNow.Month && d.FechaCreacion.Year == DateTime.UtcNow.Year);
        return $"Total de clientes activos: {total}. Nuevos este mes: {nuevosMes}.";
    }

    private async Task<string> ObtenerContextoMascotasAsync()
    {
        var total = await _db.Mascotas.CountAsync(m => m.Activo);
        var porEspecie = await _db.Mascotas
            .Where(m => m.Activo)
            .GroupBy(m => m.Especie)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToListAsync();
        var detalle = string.Join(", ", porEspecie);
        return $"Total de mascotas: {total}. Por especie: {detalle}.";
    }

    private async Task<string> ObtenerContextoConsultasAsync(string pregunta, DateTime hoy)
    {
        if (pregunta.Contains("hoy"))
        {
            var hoyCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date == hoy);
            return $"Consultas hoy: {hoyCount}.";
        }
        if (pregunta.Contains("semana"))
        {
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var semanaCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date >= inicioSemana);
            return $"Consultas esta semana: {semanaCount}.";
        }
        if (pregunta.Contains("mes"))
        {
            var mesCount = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Month == hoy.Month && c.FechaConsulta.Year == hoy.Year);
            return $"Consultas este mes: {mesCount}.";
        }
        var total = await _db.Consultas.CountAsync(c => c.Activo);
        var completadas = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Completada");
        var pendientes = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var enRevision = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "EnRevision");
        return $"Total consultas: {total}. Completadas: {completadas}. En revision: {enRevision}. Pendientes: {pendientes}.";
    }

    private async Task<string> ObtenerContextoPendientesAsync()
    {
        var consultasPend = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var consultasRev = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "EnRevision");
        var solicitudesPend = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo && s.Estado == "Pendiente");
        var comprasPend = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        return $"Pendientes - Consultas: {consultasPend}, En revision: {consultasRev}, Solicitudes adopcion: {solicitudesPend}, Compras a proveedores: {comprasPend}.";
    }

    private async Task<string> ObtenerContextoInventarioAsync()
    {
        var productos = await _db.Productos
            .Where(p => p.Activo)
            .Include(p => p.Inventarios)
            .Include(p => p.Categoria)
            .Select(p => new { p.Nombre, Stock = p.Inventarios.Any() ? p.Inventarios.First().StockActual : 0, p.StockMinimo, Categoria = p.Categoria!.Nombre })
            .ToListAsync();

        var total = productos.Count;
        var bajos = productos.Where(p => p.Stock <= p.StockMinimo).ToList();
        var sinStock = productos.Where(p => p.Stock == 0).ToList();

        var resultado = $"Total productos: {total}. Sin stock: {sinStock.Count}. Stock bajo el minimo: {bajos.Count}.";
        if (bajos.Any())
        {
            var listaBajos = string.Join(", ", bajos.Take(5).Select(p => $"{p.Nombre}({p.Stock}u)"));
            resultado += $" Productos criticos: {listaBajos}.";
        }
        return resultado;
    }

    private async Task<string> ObtenerContextoVentasAsync(string pregunta, DateTime hoy)
    {
        if (pregunta.Contains("hoy"))
        {
            var ventasHoy = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Date == hoy)
                .CountAsync();
            var montoHoy = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Total);
            return $"Ventas hoy: {ventasHoy}. Ingresos hoy: ${montoHoy:F2}.";
        }
        if (pregunta.Contains("mes"))
        {
            var ventasMes = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year)
                .CountAsync();
            var montoMes = await _db.Ventas
                .Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year)
                .SumAsync(v => v.Total);
            return $"Ventas este mes: {ventasMes}. Ingresos este mes: ${montoMes:F2}.";
        }
        var totalVentas = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pagada");
        var totalIngresos = await _db.Ventas.Where(v => v.Activo && v.Estado == "Pagada").SumAsync(v => v.Total);
        var pendientesPago = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pendiente");
        return $"Total ventas pagadas: {totalVentas}. Ingresos totales: ${totalIngresos:F2}. Ventas pendientes de pago: {pendientesPago}.";
    }

    private async Task<string> ObtenerContextoEmpleadosAsync()
    {
        var total = await _db.Empleados.CountAsync(e => e.Activo);
        var porCargo = await _db.Empleados
            .Where(e => e.Activo)
            .GroupBy(e => e.Cargo)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToListAsync();
        var detalle = string.Join(", ", porCargo);
        return $"Total empleados activos: {total}. Por cargo: {detalle}.";
    }

    private async Task<string> ObtenerContextoAdopcionAsync()
    {
        var totalAnimales = await _db.AnimalesAdopcion.CountAsync(a => a.Activo && a.Disponible);
        var solicitudesPend = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo && s.Estado == "Pendiente");
        var solicitudesTotal = await _db.SolicitudesAdopcion.CountAsync(s => s.Activo);
        return $"Animales disponibles para adopcion: {totalAnimales}. Solicitudes de adopcion pendientes: {solicitudesPend}. Total solicitudes: {solicitudesTotal}.";
    }

    private async Task<string> ObtenerContextoComprasAsync()
    {
        var pendientes = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var recibidas = await _db.Compras.CountAsync(c => c.Activo && c.Estado == "Recibida");
        var totalMes = await _db.Compras.CountAsync(c => c.Activo && c.FechaCompra.Month == DateTime.UtcNow.Month && c.FechaCompra.Year == DateTime.UtcNow.Year);
        return $"Compras pendientes: {pendientes}. Recibidas: {recibidas}. Compras este mes: {totalMes}.";
    }

    private async Task<string> ObtenerContextoTratamientosAsync()
    {
        var total = await _db.Tratamientos.CountAsync(t => t.Activo);
        var recientes = await _db.Tratamientos
            .Where(t => t.Activo)
            .OrderByDescending(t => t.FechaCreacion)
            .Take(3)
            .Select(t => t.Nombre)
            .ToListAsync();
        var detalle = recientes.Any() ? string.Join(", ", recientes) : "ninguno reciente";
        return $"Total tratamientos registrados: {total}. Ultimos: {detalle}.";
    }

    private async Task<string> ObtenerContextoResumenAsync(DateTime hoy)
    {
        var clientes = await _db.Duenos.CountAsync(d => d.Activo);
        var mascotas = await _db.Mascotas.CountAsync(m => m.Activo);
        var consultasHoy = await _db.Consultas.CountAsync(c => c.Activo && c.FechaConsulta.Date == hoy);
        var consultasPend = await _db.Consultas.CountAsync(c => c.Activo && c.Estado == "Pendiente");
        var ventasMes = await _db.Ventas.CountAsync(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year);
        var ingresosMes = await _db.Ventas.Where(v => v.Activo && v.Estado == "Pagada" && v.FechaVenta.Month == hoy.Month && v.FechaVenta.Year == hoy.Year).SumAsync(v => v.Total);
        var productos = await _db.Productos.CountAsync(p => p.Activo);
        var veterinarios = await _db.Empleados.CountAsync(e => e.Activo && e.Cargo == "Veterinario");
        var adopciones = await _db.AnimalesAdopcion.CountAsync(a => a.Activo && a.Disponible);

        return $"Resumen clinica Huellitas Felices: {clientes} clientes, {mascotas} mascotas, {veterinarios} veterinarios activos. Consultas hoy: {consultasHoy}, pendientes: {consultasPend}. Ventas este mes: {ventasMes}, ingresos: ${ingresosMes:F2}. Productos en tienda: {productos}. Animales en adopcion: {adopciones}.";
    }
}
