using HuellitasFelices.Data;
using HuellitasFelices.Models;

namespace HuellitasFelices.Services
{
    /// <summary>
    /// Servicio responsable de generar 500.000 registros distribuidos en todas las tablas.
    /// Usa inserción por lotes (batches) para no saturar la memoria ni la conexión.
    /// </summary>
    public static class CargaMasivaService
    {
        private static readonly string[] Especies = { "Perro", "Gato", "Conejo", "Hamster", "Ave" };
        private static readonly string[] RazasPerro = { "Labrador", "Bulldog", "Poodle", "Pastor Alemán", "Chihuahua", "Mestizo" };
        private static readonly string[] RazasGato = { "Persa", "Siamés", "Angora", "Bengalí", "Mestizo" };
        private static readonly string[] Cargos = { "Veterinario", "Asistente", "Recepcionista", "Auxiliar" };
        private static readonly string[] Motivos = { "Vacunación", "Desparasitación", "Revisión general", "Control de peso", "Cirugía menor", "Urgencia", "Limpieza dental" };
        private static readonly string[] MetodosPago = { "Efectivo", "Tarjeta", "Transferencia" };
        private static readonly string[] EstadosPago = { "Pagado", "Pendiente", "Anulado" };
        private static readonly string[] NombresAnimales = { "Max", "Luna", "Rocky", "Mia", "Toby", "Bella", "Thor", "Nala", "Bruno", "Coco", "Lola", "Simba", "Kira", "Zeus", "Nina" };
        private static readonly string[] Nombres = { "Juan", "María", "Carlos", "Ana", "Luis", "Carmen", "Pedro", "Laura", "José", "Rosa", "Diego", "Sofía", "Andrés", "Valentina", "Mateo" };
        private static readonly string[] Apellidos = { "García", "Pérez", "López", "Martínez", "Rodríguez", "González", "Hernández", "Torres", "Ramírez", "Flores", "Cruz", "Vargas", "Morales", "Jiménez", "Reyes" };

        public static async Task GenerarDatos(AppDbContext context)
        {
            var rng = new Random(42);

            await GenerarEmpleados(context, rng);
            await GenerarDuenos(context, rng);
            await GenerarMascotas(context, rng);
            await GenerarConsultas(context, rng);
            await GenerarTratamientos(context, rng);
            await GenerarPagos(context, rng);
            await GenerarAnimalesAdopcion(context, rng);
            await GenerarSolicitudesAdopcion(context, rng);
        }

        // ── 1.000 Empleados ──────────────────────────────────────────────────
        private static async Task GenerarEmpleados(AppDbContext context, Random rng)
        {
            if (context.Empleados.Count() >= 1000) return;

            var lote = new List<Empleado>();
            for (int i = 1; i <= 1000; i++)
            {
                lote.Add(new Empleado
                {
                    Nombre = $"{Nombres[rng.Next(Nombres.Length)]} {Apellidos[rng.Next(Apellidos.Length)]}",
                    Cargo = Cargos[rng.Next(Cargos.Length)],
                    Telefono = $"09{rng.Next(10000000, 99999999)}",
                    Salario = Math.Round((decimal)(rng.NextDouble() * 2000 + 600), 2),
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 500)
                {
                    context.Empleados.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Empleados.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 80.000 Dueños ────────────────────────────────────────────────────
        private static async Task GenerarDuenos(AppDbContext context, Random rng)
        {
            if (context.Duenos.Count() >= 80000) return;

            var lote = new List<Dueno>();
            for (int i = 1; i <= 80000; i++)
            {
                var nombre = Nombres[rng.Next(Nombres.Length)];
                var apellido = Apellidos[rng.Next(Apellidos.Length)];
                lote.Add(new Dueno
                {
                    Nombre = $"{nombre} {apellido}",
                    Telefono = $"09{rng.Next(10000000, 99999999)}",
                    Email = $"{nombre.ToLower()}.{apellido.ToLower()}{i}@mail.com",
                    Direccion = $"Calle {rng.Next(1, 500)} y Av. {rng.Next(1, 100)}",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 2000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Duenos.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Duenos.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 100.000 Mascotas ─────────────────────────────────────────────────
        private static async Task GenerarMascotas(AppDbContext context, Random rng)
        {
            if (context.Mascotas.Count() >= 100000) return;

            var duenoIds = context.Duenos.Select(d => d.Id).ToList();

            var lote = new List<Mascota>();
            for (int i = 1; i <= 100000; i++)
            {
                var especie = Especies[rng.Next(2)]; // Solo perro o gato
                var raza = especie == "Perro"
                    ? RazasPerro[rng.Next(RazasPerro.Length)]
                    : RazasGato[rng.Next(RazasGato.Length)];

                lote.Add(new Mascota
                {
                    Nombre = NombresAnimales[rng.Next(NombresAnimales.Length)],
                    Especie = especie,
                    Raza = raza,
                    Edad = rng.Next(0, 15),
                    Peso = Math.Round((decimal)(rng.NextDouble() * 30 + 1), 1),
                    DuenoId = duenoIds[rng.Next(duenoIds.Count)],
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 1500)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Mascotas.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Mascotas.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 150.000 Consultas ────────────────────────────────────────────────
        private static async Task GenerarConsultas(AppDbContext context, Random rng)
        {
            if (context.Consultas.Count() >= 150000) return;

            var mascotaIds = context.Mascotas.Select(m => m.Id).ToList();

            var lote = new List<Consulta>();
            for (int i = 1; i <= 150000; i++)
            {
                lote.Add(new Consulta
                {
                    Motivo = Motivos[rng.Next(Motivos.Length)],
                    Diagnostico = "Diagnóstico registrado en consulta",
                    Costo = Math.Round((decimal)(rng.NextDouble() * 100 + 10), 2),
                    FechaConsulta = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    MascotaId = mascotaIds[rng.Next(mascotaIds.Count)],
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Consultas.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Consultas.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 100.000 Tratamientos ─────────────────────────────────────────────
        private static async Task GenerarTratamientos(AppDbContext context, Random rng)
        {
            if (context.Tratamientos.Count() >= 100000) return;

            var consultaIds = context.Consultas.Select(c => c.Id).ToList();
            string[] nombresTrat = { "Vacuna Antirrábica", "Desparasitación", "Antibiótico", "Antiinflamatorio", "Vitaminas", "Suero", "Limpieza dental" };
            string[] medicamentos = { "Amoxicilina", "Drontal", "Rabisin", "Meloxicam", "Vitovet", "N/A" };

            var lote = new List<Tratamiento>();
            for (int i = 1; i <= 100000; i++)
            {
                lote.Add(new Tratamiento
                {
                    Nombre = nombresTrat[rng.Next(nombresTrat.Length)],
                    Descripcion = "Tratamiento aplicado según diagnóstico",
                    Costo = Math.Round((decimal)(rng.NextDouble() * 50 + 5), 2),
                    Medicamento = medicamentos[rng.Next(medicamentos.Length)],
                    ConsultaId = consultaIds[rng.Next(consultaIds.Count)],
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Tratamientos.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Tratamientos.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 50.000 Pagos ─────────────────────────────────────────────────────
        private static async Task GenerarPagos(AppDbContext context, Random rng)
        {
            if (context.Pagos.Count() >= 50000) return;

            var duenoIds = context.Duenos.Select(d => d.Id).ToList();

            var lote = new List<Pago>();
            for (int i = 1; i <= 50000; i++)
            {
                lote.Add(new Pago
                {
                    Monto = Math.Round((decimal)(rng.NextDouble() * 200 + 10), 2),
                    MetodoPago = MetodosPago[rng.Next(MetodosPago.Length)],
                    Estado = EstadosPago[rng.Next(EstadosPago.Length)],
                    FechaPago = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    DuenoId = duenoIds[rng.Next(duenoIds.Count)],
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 1000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Pagos.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.Pagos.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 10.000 Animales en adopción ──────────────────────────────────────
        private static async Task GenerarAnimalesAdopcion(AppDbContext context, Random rng)
        {
            if (context.AnimalesAdopcion.Count() >= 10000) return;

            var lote = new List<AnimalAdopcion>();
            for (int i = 1; i <= 10000; i++)
            {
                var especie = Especies[rng.Next(2)];
                lote.Add(new AnimalAdopcion
                {
                    Nombre = NombresAnimales[rng.Next(NombresAnimales.Length)],
                    Especie = especie,
                    Raza = "Mestizo",
                    EdadAproximada = rng.Next(0, 10),
                    Descripcion = "Animal en espera de un hogar",
                    Disponible = rng.Next(2) == 0,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 500)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 500)
                {
                    context.AnimalesAdopcion.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.AnimalesAdopcion.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }

        // ── 9.000 Solicitudes de adopción ────────────────────────────────────
        private static async Task GenerarSolicitudesAdopcion(AppDbContext context, Random rng)
        {
            if (context.SolicitudesAdopcion.Count() >= 9000) return;

            var animalIds = context.AnimalesAdopcion.Select(a => a.Id).ToList();
            string[] estados = { "Pendiente", "Aprobada", "Rechazada" };

            var lote = new List<SolicitudAdopcion>();
            for (int i = 1; i <= 9000; i++)
            {
                var nombre = Nombres[rng.Next(Nombres.Length)];
                var apellido = Apellidos[rng.Next(Apellidos.Length)];
                lote.Add(new SolicitudAdopcion
                {
                    NombreSolicitante = $"{nombre} {apellido}",
                    Telefono = $"09{rng.Next(10000000, 99999999)}",
                    Email = $"{nombre.ToLower()}{i}@mail.com",
                    Estado = estados[rng.Next(estados.Length)],
                    FechaSolicitud = DateTime.UtcNow.AddDays(-rng.Next(1, 365)),
                    AnimalAdopcionId = animalIds[rng.Next(animalIds.Count)],
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-rng.Next(1, 365)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 500)
                {
                    context.SolicitudesAdopcion.AddRange(lote);
                    await context.SaveChangesAsync();
                    lote.Clear();
                }
            }
            if (lote.Count > 0)
            {
                context.SolicitudesAdopcion.AddRange(lote);
                await context.SaveChangesAsync();
            }
        }
    }
}
