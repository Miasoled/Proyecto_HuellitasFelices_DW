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

    private static readonly Dictionary<string[], string> ConocimientoGeneral = new()
    {
        {
            new[] { "diarrea", "diarreico", "diarreica", "heces blandas", "heces liquidas", "popo blanda" },
            "CUIDADO GENERAL - DIARREA EN MASCOTAS: " +
            "Causas comunes: cambio de alimento, ingesta de algo indigesto, infecciones virales o bacterianas, parasitosis, estr\u00e9s. " +
            "Primeros pasos: Ayunar 12-24 horas (adultos), luego ofrecer arroz blanco hervido sin condimentos mezclado con pollo desmenuzado. " +
            "Mantener hidrataci\u00f3n constante con agua fresca. Si hay sangre, v\u00f3mitos o dura m\u00e1s de 48 horas, acudir al veterinario inmediatamente. " +
            "En cachorros, la diarrea puede ser grave y deshidratar r\u00e1pidamente, buscar atenci\u00f3n profesional urgente."
        },
        {
            new[] { "vomito", "vomitar", "v\u00f3mito", "regurgitar", "devolver" },
            "CUIDADO GENERAL - VOMITO EN MASCOTAS: " +
            "Causas comunes: comidas inapropiadas, sobrealimentaci\u00f3n, infecciones, par\u00e1sitos, cuerpos extra\u00f1os. " +
            "Primeros pasos: Retener alimento 12-24 horas, ofrecer peque\u00f1as cantidades de agua frecuentemente. " +
            "Despu\u00e9s del ayuno, ofrecer dieta blanda (arroz con pollo) en porciones peque\u00f1as. " +
            "Si el v\u00f3mito contiene sangre, es continuo o dura m\u00e1s de 24 horas, acudir al veterinario."
        },
        {
            new[] { "vacuna", "vacunacion", "vacunar", "vacunas", "calendario" },
            "CUIDADO GENERAL - VACUNACION: " +
            "Perros: Primera vacuna a las 6-8 semanas, refuerzos cada 3-4 semanas hasta las 16 semanas. " +
            "Refuerzo anual despu\u00e9s. Vacunas b\u00e1sicas: moquillo, parvovirus, distemper, hepatitis, rabia. " +
            "Gatos: Primera vacuna a las 8-9 semanas, refuerzo a las 12 semanas. " +
            "Vacunas b\u00e1sicas: panleucopenia, calicivirus, herpesvirus, rabia. Consultar calendario con el veterinario."
        },
        {
            new[] { "desparasitar", "desparasitacion", "parasitos", "gusanos", "lombrices", "pulgas", "garrapatas", "desparasitante" },
            "CUIDADO GENERAL - DESPARASITACION: " +
            "Internos: Desparasitar cada 3 meses en adultos, cada 2 semanas en cachorros hasta los 3 meses. " +
            "Externos: Antipulgas y antigarrapatas seg\u00fan el producto indicado por el veterinario. " +
            "Se\u00f1ales de par\u00e1sitos: abdomen hinchado, diarrea con moco o sangre, pelo opaco, prurito anal. " +
            "Consultar al veterinario para el antiparasitario adecuado seg\u00fan peso y especie."
        },
        {
            new[] { "alergia", "alergico", "alergica", "picaz\u00f3n", "rascarse", "ronchas" },
            "CUIDADO GENERAL - ALERGIAS EN MASCOTAS: " +
            "Causas: alimentos, pulgas, ambientales (polvo, polen), qu\u00edmicos. " +
            "Se\u00f1ales: rascado excesivo, enrojecimiento de piel, loss de pelo, inflamaci\u00f3n de o\u00eddos. " +
            "Primeros pasos: Identificar y eliminar la fuente. Ba\u00f1os con jab\u00f3n hipoalerg\u00e9nico puede aliviar. " +
            "El veterinario puede recortar antihistam\u00ednicos o tratamientos espec\u00edficos."
        },
        {
            new[] { "esterilizar", "esterilizacion", "castracion", "castrar", "covar", "ovariohisterectomia" },
            "CUIDADO GENERAL - ESTERILIZACION: " +
            "Recomendada a partir de los 6 meses de edad. Beneficios: previene tumores mamarios, piometra, " +
            "reduce comportamientos no deseados (marcaje, escape, celos). " +
            "Procedimiento quir\u00fargico con anestesia general. Cuidados postoperatorios: " +
            "reposo 7-10 d\u00edas, usar cono Elizabethano, controlar la herida diariamente."
        },
        {
            new[] { "diente", "dientes", "dental", "limpieza dental", "sangrado de encias", "mal aliento", "halitosis" },
            "CUIDADO GENERAL - SALUD DENTAL: " +
            "Los dientes deben limpiarse semanalmente con cepillo y pasta dental veterinaria. " +
            "Se\u00f1ales de problema: mal aliento, sangrado de encias, dificultad para comer, acumulaci\u00f3n de sarro. " +
            "La limpieza profesional requiere anestesia. Prevenci\u00f3n: juguetes dentales, golosinas dentales, cepillado regular."
        },
        {
            new[] { "ojos", "ojo", "ocular", "lagrimeo", "secreccion ocular", "conjuntivitis" },
            "CUIDADO GENERAL - SALUD OCULAR: " +
            "Se\u00f1ales de alarma: secreci\u00f3n abundante, enrojecimiento, hinchaz\u00f3n, opacidad del ojo, " +
            "rascado constante del ojo. " +
            "Limpieza: usar soluci\u00f3n fisiol\u00f3gica o l\u00e1grimas artificiales veterinarias con una gasa est\u00e9ril. " +
            "No usar remedios caseros. Si persiste, acudir al veterinario."
        },
        {
            new[] { "oreja", "orejas", "auditivo", "otitis", "cera", "mal olor oreja" },
            "CUIDADO GENERAL - SALUD AUDITIVA: " +
            "Limpiar las orejas semanalmente con limpiador otol\u00f3gico veterinario y gasa est\u00e9ril. " +
            "Nunca insertar hisopos. Se\u00f1ales de infecci\u00f3n: mal olor, secreci\u00f3n oscura, sacudida de cabeza, rascado. " +
            "Razas con orejas ca\u00eddas son m\u00e1s propensas a infecciones. Consultar al veterinario si hay s\u00edntomas."
        },
        {
            new[] { "herida", "heridas", "cortada", "cortado", "sangre", "sangrado", "golpe" },
            "CUIDADO GENERAL - HERIDAS: " +
            "Primeros auxilios: limpiar con soluci\u00f3n fisiol\u00f3gica o agua limpia. " +
            "Aplicar presi\u00f3n con gasa est\u00e9ril si sangra. No usar alcohol o per\u00f3xido directamente. " +
            "Cubrir con vendaje limpio. Si la herida es profunda, sangra mucho o est\u00e1 infectada (pus, hinchaz\u00f3n, mal olor), " +
            "acudir al veterinario de inmediato."
        },
        {
            new[] { "peso", "obesidad", "gordo", "adelgazar", "dieta", "alimentacion", "comida" },
            "CUIDADO GENERAL - ALIMENTACION Y PESO: " +
            "Alimentar con croquetas de calidad seg\u00fan la edad y tama\u00f3no. " +
            "Porciones recomendadas: 2-3% del peso corporal al d\u00eda para adultos. " +
            "Evitar: chocolate, uvas, cebolla, ajo, huesos cocidos, leche, az\u00facar. " +
            "Ejercicio diario: al menos 30 minutos de caminata para perros. " +
            "Controlar peso regularmente. La obesidad causa diabetes, problemas articulares y card\u00edacos."
        },
        {
            new[] { "cachorro", "cachorros", "cachorra", "bebe", "bebe", "bebe perro", "bebe gato" },
            "CUIDADO GENERAL - CUIDADO DE CACHORROS: " +
            "Alimentaci\u00f3n: croquetas especiales para cachorros, 3-4 veces al d\u00eda hasta los 6 meses, luego 2 veces. " +
            "Vacunaci\u00f3n: iniciar a las 6-8 semanas. Desparasitaci\u00f3n: cada 2 semanas hasta los 3 meses, luego cada 3 meses. " +
            "Socializaci\u00f3n: exponer a diferentes personas, lugares y sonidos entre las 3-14 semanas. " +
            "Higiene: ba\u00f1os suaves cada 15-20 d\u00edas. No ba\u00f1ar hasta completar el esquema de vacunaci\u00f3n."
        },
        {
            new[] { "embarazo", "pre\u00f1ada", "gestacion", "parto", "cuidado embarazo" },
            "CUIDADO GENERAL - EMBARAZO EN MASCOTAS: " +
            "Duraci\u00f3n: perros 58-68 d\u00edas, gatos 63-67 d\u00edas. " +
            "Se\u00f1ales: aumento de peso, hinchaz\u00f3n del pez\u00f3n, cambios de comportamiento. " +
            "Cuidados: alimentaci\u00f3n de alta calidad, ejercicio moderado, ambiente tranquilo. " +
            "Eco o radiograf\u00eda para confirmar y contar cr\u00edas. Acudir al veterinario si hay sangrado, fiebre o dificultad para parir."
        },
        {
            new[] { "fiebre", "temperatura", "calentura", "escalofrios", "temblor" },
            "CUIDADO GENERAL - FIEBRE EN MASCOTAS: " +
            "Temperatura normal: perros 38.3-39.2\u00b0C, gatos 38.1-39.2\u00b0C. " +
            "Se\u00f1ales: letargo, p\u00e9rdida de apetito, temblores, nariz seca. " +
            "Medir con term\u00f3metro rectal. Aplicar compresas fr\u00edas en patas. " +
            "Nunca dar ibuprofeno, paracetamol o medicamentos humanos (son t\u00f3xicos para mascotas). " +
            "Si la fiebre supera 39.5\u00b0C o dura m\u00e1s de 24 horas, acudir al veterinario."
        },
        {
            new[] { "tos", "toser", "estornudo", "gripe", "resfriado", "resfriada" },
            "CUIDADO GENERAL - PROBLEMAS RESPIRATORIOS: " +
            "Causas: infecciones virales o bacterianas, alergias, cuerpos extra\u00f1os, bronquitis. " +
            "Cuidados: mantener en ambiente c\u00e1lido y libre de corrientes. " +
            "Hidrataci\u00f3n constante. Vapor de agua puede ayudar a despejar v\u00edas respiratorias. " +
            "Si hay dificultad para respirar, flema con sangre o dura m\u00e1s de 3 d\u00edas, acudir al veterinario urgente."
        },
        {
            new[] { "cojera", "cojo", "coja", "patita", "camina mal", "no camina", "articulacion", "artritis" },
            "CUIDADO GENERAL - PROBLEMAS ARTICULARES: " +
            "Causas: traumatismos, displasia, artritis, lesiones de ligamentos. " +
            "Primeros pasos: reposo, no forzar la movilidad. Aplicar compresas fr\u00edas las primeras 48 horas. " +
            "Si no mejora en 24-48 horas o hay hinchaz\u00f3n, acudir al veterinario. " +
            "Prevenci\u00f3n: mantener peso ideal, ejercicio moderado, suplementos de glucosamina para razas grandes."
        },
        {
            new[] { "comez\u00f3n", "picar", "picaz\u00f3n", "rasgu\u00f1o", "se rasca mucho" },
            "CUIDADO GENERAL - PRURITO (COMEZ\u00d3N): " +
            "Causas: pulgas, alergias, hongos, piel seca, par\u00e1sitos externos. " +
            "Revisar pelaje en busca de pulgas o huevos (puntitos negros). " +
            "Ba\u00f1os con jab\u00f3n antiinflamatorio o avena coloidal pueden aliviar. " +
            "No dejar que la mascota se rasque en exceso (usar cono si es necesario). " +
            "Consultar al veterinario para diagn\u00f3stico y tratamiento adecuado."
        },
        {
            new[] { "transporte", "viaje", "auto", "coche", "mareo", "marea", "estres viaje" },
            "CUIDADO GENERAL - TRANSPORTE Y VIAJE: " +
            "Mareo por transporte: no alimentar 2-3 horas antes del viaje. " +
            "Ventilar el veh\u00edculo, hacer paradas cada 2 horas. " +
            "Usar transportadora segura con ventilaci\u00f3n. " +
            "Para viajes largos, llevar: agua, comida, pl\u00e1tico, toallas, documentaci\u00f3n veterinaria, vacunas al d\u00eda."
        }
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

        if (contextos.Count > 0)
            return string.Join(" | ", contextos);

        foreach (var (keywords, consejo) in ConocimientoGeneral)
        {
            if (keywords.Any(k => pregunta.Contains(k)))
            {
                return consejo;
            }
        }

        return "No hay datos en la base de datos para esta pregunta. Puedes preguntar sobre el estado de la clinica (mascotas, consultas, ventas, inventario, etc.) o sobre cuidados generales de mascotas (diarrea, vacunas, alimentacion, desparasitacion, etc.).";
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
