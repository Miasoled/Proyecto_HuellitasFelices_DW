using HuellitasFelices.Data;
using HuellitasFelices.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace HuellitasFelices.Services
{
    /// <summary>
    /// Genera registros adicionales en la base de datos hasta alcanzar 1.000.000 en total.
    /// Usa lotes (batches) y limpia el Change Tracker de EF Core para no saturar la memoria.
    /// Utiliza la librería Bogus con localización en español para generar datos variados y realistas.
    /// </summary>
    public static class CargaMasivaService
    {
        // Listas de datos base para combinaciones y coherencia veterinaria
        private static readonly string[] CargosEmpleado = { "Veterinario", "Asistente", "Recepcionista", "Auxiliar" };
        private static readonly string[] RazasPerro = {
            "Labrador", "Bulldog", "Poodle", "Pastor Alemán", "Chihuahua", "Mestizo",
            "Golden Retriever", "Beagle", "Rottweiler", "Dálmata", "Schnauzer", "Boxer",
            "Shih Tzu", "Pomerania", "Husky Siberiano", "Cocker Spaniel", "Yorkshire",
            "Pitbull", "Doberman", "Border Collie", "San Bernardo", "Pug"
        };
        
        private static readonly string[] RazasGato = {
            "Persa", "Siamés", "Angora", "Bengalí", "Mestizo", "Maine Coon",
            "Ragdoll", "Sphynx", "Abisinio", "Birmano", "Europeo", "Scottish Fold",
            "Himalayo", "Siberiano", "Azul Ruso"
        };

        private static readonly string[] NombresAnimales = {
            "Max", "Luna", "Rocky", "Mia", "Toby", "Bella", "Thor", "Nala", "Bruno", "Coco",
            "Lola", "Simba", "Kira", "Zeus", "Nina", "Cleo", "Rex", "Milo", "Lily", "Oscar",
            "Daisy", "Charlie", "Lucy", "Buddy", "Molly", "Bear", "Zoe", "Duke", "Maggie",
            "Cooper", "Sadie", "Tucker", "Bailey", "Stella", "Winston", "Rosie", "Bentley",
            "Gracie", "Harley", "Sophie", "Oliver", "Lulu", "Finn", "Ruby", "Diesel", "Penny"
        };

        private static readonly string[] FotosPerro = {
            "https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=400",
            "https://images.unsplash.com/photo-1552053831-71594a27632d?w=400",
            "https://images.unsplash.com/photo-1518717758536-85ae29035b6d?w=400",
            "https://images.unsplash.com/photo-1561037404-61cd46aa615b?w=400",
            "https://images.unsplash.com/photo-1574144611937-0df059b5ef3e?w=400",
            "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=400"
        };

        private static readonly string[] FotosGato = {
            "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400",
            "https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=400",
            "https://images.unsplash.com/photo-1592194996308-7b43878e84a6?w=400",
            "https://images.unsplash.com/photo-1533743983669-94fa5c4338ec?w=400",
            "https://images.unsplash.com/photo-1571566882372-1598d88abd90?w=400"
        };

        private static readonly string[] FotosOtros = {
            "https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=400",
            "https://images.unsplash.com/photo-1548767797-d8c844163c4a?w=400",
            "https://images.unsplash.com/photo-1444464666168-49d633b86797?w=400"
        };

        // Metas de registros para alcanzar exactamente 1.000.000 en total
        private const int MetaEmpleados = 2000;
        private const int MetaDuenos = 160000;
        private const int MetaMascotas = 200000;
        private const int MetaConsultas = 300000;
        private const int MetaTratamientos = 200000;
        private const int MetaAnimalesAdopcion = 20000;
        private const int MetaSolicitudesAdopcion = 18000;

        public static async Task GenerarDatos(AppDbContext context)
        {
            var faker = new Faker("es");

            await GenerarEmpleados(context, faker);
            await GenerarDuenos(context, faker);
            await GenerarMascotas(context, faker);
            await GenerarConsultas(context, faker);
            await GenerarTratamientos(context, faker);
            await GenerarAnimalesAdopcion(context, faker);
            await GenerarSolicitudesAdopcion(context, faker);
        }

        // ── 1. 2.000 Empleados ────────────────────────────────────────────────
        private static async Task GenerarEmpleados(AppDbContext context, Faker faker)
        {
            int actual = await context.Empleados.CountAsync();
            if (actual >= MetaEmpleados) return;

            var lote = new List<Empleado>();
            int faltantes = MetaEmpleados - actual;

            for (int i = 1; i <= faltantes; i++)
            {
                var cargo = faker.Random.ListItem(CargosEmpleado);
                var salario = cargo == "Veterinario" ? faker.Random.Number(1500, 2500)
                            : cargo == "Asistente"   ? faker.Random.Number(800, 1200)
                            : faker.Random.Number(700, 1000);
                
                var esHombre = faker.Random.Bool();
                var nombre = esHombre 
                    ? $"{faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male)} {faker.Name.LastName()}" 
                    : $"{faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female)} {faker.Name.LastName()}";

                lote.Add(new Empleado
                {
                    Nombre = nombre,
                    Cargo = cargo,
                    Telefono = faker.Phone.PhoneNumber("09########"),
                    Salario = salario,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-faker.Random.Number(30, 1200)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 500)
                {
                    context.Empleados.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.Empleados.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 2. 160.000 Dueños ────────────────────────────────────────────────
        private static async Task GenerarDuenos(AppDbContext context, Faker faker)
        {
            int actual = await context.Duenos.CountAsync();
            if (actual >= MetaDuenos) return;

            var lote = new List<Dueno>();
            int faltantes = MetaDuenos - actual;

            for (int i = 1; i <= faltantes; i++)
            {
                var esHombre = faker.Random.Bool();
                var firstName = esHombre 
                    ? faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male) 
                    : faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female);
                var lastName1 = faker.Name.LastName();
                var lastName2 = faker.Name.LastName();
                var fullName = $"{firstName} {lastName1} {lastName2}";

                // Generar email consistente y único agregando un número aleatorio
                var email = faker.Internet.Email(firstName, lastName1).ToLower();
                var atIndex = email.IndexOf('@');
                if (atIndex != -1)
                {
                    email = email.Insert(atIndex, faker.Random.Number(1000, 999999).ToString());
                }

                lote.Add(new Dueno
                {
                    Nombre = fullName,
                    Telefono = faker.Phone.PhoneNumber("09########"),
                    Email = email,
                    Direccion = $"{faker.Address.StreetName()} N{faker.Random.Number(1, 999)} y Calle {faker.Address.StreetName()}",
                    Activo = faker.Random.Number(100) > 3,
                    FechaCreacion = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 2000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Duenos.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.Duenos.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 3. 200.000 Mascotas ──────────────────────────────────────────────
        private static async Task GenerarMascotas(AppDbContext context, Faker faker)
        {
            int actual = await context.Mascotas.CountAsync();
            if (actual >= MetaMascotas) return;

            var duenoIds = await context.Duenos.AsNoTracking().Select(d => d.Id).ToListAsync();
            if (duenoIds.Count == 0) return;

            var lote = new List<Mascota>();
            int faltantes = MetaMascotas - actual;

            for (int i = 1; i <= faltantes; i++)
            {
                var especie = faker.Random.ListItem(new[] { "Perro", "Gato", "Conejo", "Ave", "Hamster", "Tortuga" });
                var raza = especie switch
                {
                    "Perro" => faker.Random.ListItem(RazasPerro),
                    "Gato" => faker.Random.ListItem(RazasGato),
                    "Conejo" => faker.Random.ListItem(new[] { "Enano", "Cabeza de León", "Angora", "Belier", "Mestizo" }),
                    "Ave" => faker.Random.ListItem(new[] { "Canario", "Periquito", "Ninfa", "Loro", "Agapornis", "Mestizo" }),
                    "Hamster" => faker.Random.ListItem(new[] { "Ruso", "Dorado", "Roborovski", "Sírio" }),
                    _ => "Mestizo"
                };

                var edad = faker.Random.Number(0, 15);
                var peso = especie switch
                {
                    "Perro" => Math.Round((decimal)(faker.Random.Double() * 38 + 2), 1),
                    "Gato" => Math.Round((decimal)(faker.Random.Double() * 6 + 1), 1),
                    _ => Math.Round((decimal)(faker.Random.Double() * 3 + 0.2), 1)
                };

                var nombre = faker.Random.Bool(0.6f) 
                    ? faker.Random.ListItem(NombresAnimales) 
                    : faker.Name.FirstName();

                lote.Add(new Mascota
                {
                    Nombre = nombre,
                    Especie = especie,
                    Raza = raza,
                    Sexo = faker.Random.ListItem(new[] { "Macho", "Hembra" }),
                    FechaNacimiento = DateTime.UtcNow.AddYears(-edad),
                    Peso = peso,
                    DuenoId = faker.Random.ListItem(duenoIds),
                    Activo = faker.Random.Number(100) > 2,
                    FechaCreacion = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 1800)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Mascotas.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.Mascotas.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 4. 300.000 Consultas ─────────────────────────────────────────────
        private static async Task GenerarConsultas(AppDbContext context, Faker faker)
        {
            int actual = await context.Consultas.CountAsync();
            if (actual >= MetaConsultas) return;

            var mascotaIds = await context.Mascotas.AsNoTracking().Select(m => m.Id).ToListAsync();
            if (mascotaIds.Count == 0) return;

            var lote = new List<Consulta>();
            int faltantes = MetaConsultas - actual;

            for (int i = 1; i <= faltantes; i++)
            {
                var costo = Math.Round((decimal)(faker.Random.Double() * 90 + 10), 2);
                var fecha = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 1095));
                var motivo = GenerarMotivoRandom(faker);
                var diagnostico = GenerarDiagnosticoRandom(faker);

                lote.Add(new Consulta
                {
                    Motivo = motivo,
                    Diagnostico = diagnostico,
                    Costo = costo,
                    FechaConsulta = fecha,
                    MascotaId = faker.Random.ListItem(mascotaIds),
                    Activo = faker.Random.Number(100) > 2,
                    FechaCreacion = fecha,
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Consultas.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.Consultas.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 5. 200.000 Tratamientos ──────────────────────────────────────────
        private static async Task GenerarTratamientos(AppDbContext context, Faker faker)
        {
            int actual = await context.Tratamientos.CountAsync();
            if (actual >= MetaTratamientos) return;

            var consultaIds = await context.Consultas.AsNoTracking().Select(c => c.Id).ToListAsync();
            if (consultaIds.Count == 0) return;

            var lote = new List<Tratamiento>();
            int faltantes = MetaTratamientos - actual;

            var nombresTratamiento = new[] {
                "Vacuna Antirrábica", "Vacuna Quíntuple", "Desparasitación Interna",
                "Desparasitación Externa", "Limpieza Otológica", "Tratamiento Antibiótico",
                "Terapia de Hidratación", "Tratamiento Antiinflamatorio", "Protector Gástrico",
                "Suplemento Articular", "Tratamiento Dermatológico", "Profilaxis Dental"
            };
            var medicamentos = new[] {
                "Amoxicilina", "Metronidazol", "Meloxicam", "Tramadol", "Drontal Plus",
                "Frontline Tri-Act", "Vitovet", "Omeprazol", "Cefalexina", "Prednisona",
                "Ivermectina", "Enrofloxacina"
            };

            for (int i = 1; i <= faltantes; i++)
            {
                var medicamento = faker.Random.ListItem(medicamentos);
                var dosis = faker.Random.ListItem(new[] { "1 tableta", "1/2 tableta", "2 ml", "5 ml", "1 cápsula" });
                var frecuencia = faker.Random.ListItem(new[] { "8 horas", "12 horas", "24 horas" });
                var duracion = faker.Random.ListItem(new[] { "3 días", "5 días", "7 días", "10 días" });
                var descripcion = $"Administrar {dosis} de {medicamento} cada {frecuencia} durante {duracion}.";

                lote.Add(new Tratamiento
                {
                    Nombre = faker.Random.ListItem(nombresTratamiento),
                    Descripcion = descripcion,
                    Costo = Math.Round((decimal)(faker.Random.Double() * 45 + 5), 2),
                    Medicamento = medicamento,
                    ConsultaId = faker.Random.ListItem(consultaIds),
                    Activo = faker.Random.Number(100) > 2,
                    FechaCreacion = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 1000)),
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 1000)
                {
                    context.Tratamientos.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.Tratamientos.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 6. 20.000 Animales en Adopción ───────────────────────────────────
        private static async Task GenerarAnimalesAdopcion(AppDbContext context, Faker faker)
        {
            int actual = await context.AnimalesAdopcion.CountAsync();
            if (actual >= MetaAnimalesAdopcion) return;

            var lote = new List<AnimalAdopcion>();
            int faltantes = MetaAnimalesAdopcion - actual;

            for (int i = 1; i <= faltantes; i++)
            {
                var especie = faker.Random.ListItem(new[] { "Perro", "Gato", "Conejo", "Ave", "Hamster" });
                var raza = especie switch
                {
                    "Perro" => faker.Random.ListItem(RazasPerro),
                    "Gato" => faker.Random.ListItem(RazasGato),
                    "Conejo" => faker.Random.ListItem(new[] { "Enano", "Cabeza de León", "Mestizo" }),
                    _ => "Mestizo"
                };

                string? foto = null;
                if (faker.Random.Bool(0.7f))
                {
                    foto = especie == "Perro" ? faker.Random.ListItem(FotosPerro)
                         : especie == "Gato"  ? faker.Random.ListItem(FotosGato)
                         : faker.Random.ListItem(FotosOtros);
                }

                // Generar descripción variada combinando rasgos de personalidad y necesidades
                var rasgos = new[] { "Muy juguetón", "Cariñoso", "Tranquilo", "Activo y con mucha energía", "Sociable con otros animales", "Un poco tímido pero muy noble", "Independiente" };
                var compatibilidad = new[] { "ideal para familias con niños.", "perfecto para vivir en departamento.", "se lleva excelente con otros animales.", "le encanta correr y jugar al aire libre.", "busca un hogar con paciencia y cariño." };
                var extras = new[] { "Ya se encuentra esterilizado y vacunado.", "Rescatado con mucho amor.", "Listo para dar cariño a su nueva familia." };
                
                var descripcion = $"{faker.Random.ListItem(rasgos)}, {faker.Random.ListItem(compatibilidad)} {faker.Random.ListItem(extras)}";

                lote.Add(new AnimalAdopcion
                {
                    Nombre = faker.Random.ListItem(NombresAnimales),
                    Especie = especie,
                    Raza = raza,
                    EdadAproximada = faker.Random.Number(0, 10),
                    Descripcion = descripcion,
                    Disponible = faker.Random.Bool(0.7f),
                    FotoUrl = foto,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 600)),
                    FechaActualizacion = DateTime.UtcNow.AddDays(-faker.Random.Number(0, 30))
                });

                if (lote.Count == 500)
                {
                    context.AnimalesAdopcion.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.AnimalesAdopcion.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── 8. 18.000 Solicitudes de Adopción ─────────────────────────────────
        private static async Task GenerarSolicitudesAdopcion(AppDbContext context, Faker faker)
        {
            int actual = await context.SolicitudesAdopcion.CountAsync();
            if (actual >= MetaSolicitudesAdopcion) return;

            var animalIds = await context.AnimalesAdopcion.AsNoTracking().Select(a => a.Id).ToListAsync();
            if (animalIds.Count == 0) return;

            var lote = new List<SolicitudAdopcion>();
            int faltantes = MetaSolicitudesAdopcion - actual;
            string[] estados = { "Pendiente", "Aprobada", "Rechazada" };

            for (int i = 1; i <= faltantes; i++)
            {
                var esHombre = faker.Random.Bool();
                var firstName = esHombre 
                    ? faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male) 
                    : faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female);
                var lastName = faker.Name.LastName();
                var fecha = DateTime.UtcNow.AddDays(-faker.Random.Number(1, 400));

                var email = faker.Internet.Email(firstName, lastName).ToLower();
                var atIndex = email.IndexOf('@');
                if (atIndex != -1)
                {
                    email = email.Insert(atIndex, faker.Random.Number(100, 9999).ToString());
                }

                lote.Add(new SolicitudAdopcion
                {
                    NombreSolicitante = $"{firstName} {lastName}",
                    Telefono = faker.Phone.PhoneNumber("09########"),
                    Email = email,
                    Estado = faker.Random.ListItem(estados),
                    FechaSolicitud = fecha,
                    AnimalAdopcionId = faker.Random.ListItem(animalIds),
                    Activo = true,
                    FechaCreacion = fecha,
                    FechaActualizacion = DateTime.UtcNow
                });

                if (lote.Count == 500)
                {
                    context.SolicitudesAdopcion.AddRange(lote);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    lote.Clear();
                }
            }

            if (lote.Count > 0)
            {
                context.SolicitudesAdopcion.AddRange(lote);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }
        }

        // ── Métodos Auxiliares de Generación ────────────────────────────────
        private static string GenerarMotivoRandom(Faker faker)
        {
            var intros = new[] { "Revisión por", "Paciente presenta", "Dueño reporta", "Control de", "Consulta de urgencia por" };
            var sintomas = new[] { 
                "fiebre alta", "vómitos y diarrea", "tos persistente", "falta de apetito", 
                "dolor en la pata trasera", "picazón constante en la piel", "herida por pelea", 
                "secreción ocular", "dificultad para respirar", "pérdida de pelo", 
                "infección en el oído izquierdo", "debilidad general", "estornudos frecuentes", 
                "problemas urinarios" 
            };
            var detalles = new[] { "desde ayer", "hace 3 días", "tras paseo en el parque", "después de comer comida casera", "de forma intermitente", "sin causa aparente" };

            if (faker.Random.Bool(0.15f))
            {
                return faker.Random.ListItem(new[] { "Vacunación anual", "Desparasitación de rutina", "Control de peso", "Revisión general de salud", "Profilaxis dental" });
            }

            return $"{faker.Random.ListItem(intros)} {faker.Random.ListItem(sintomas)} {faker.Random.ListItem(detalles)}";
        }

        private static string GenerarDiagnosticoRandom(Faker faker)
        {
            var clinicos = new[] { 
                "Gastroenteritis infecciosa", "Dermatitis atópica", "Otitis bilateral", 
                "Bronquitis leve", "Infección urinaria", "Esguince en articulación", 
                "Conjuntivitis infecciosa", "Parasitosis intestinal", 
                "Reacción alérgica alimentaria", "Gingivitis moderada" 
            };
            var acciones = new[] { 
                "Se prescribe antibiótico oral y dieta blanda", 
                "Se recomienda baño medicado y antiinflamatorios", 
                "Se aplica limpieza de canal auditivo con gotas óticas", 
                "Se prescribe antiparasitario de amplio espectro", 
                "Se recomienda reposo absoluto por 5 días", 
                "Se prescribe colirio oftálmico cada 8 horas", 
                "Se realiza profilaxis dental bajo sedación", 
                "Se indica hidratación oral abundante" 
            };
            var controles = new[] { 
                "control en 7 días.", "control en caso de persistir los síntomas.", 
                "seguimiento telefónico en 48 horas.", "revisión el próximo mes.", 
                "nueva cita para exámenes de laboratorio." 
            };

            if (faker.Random.Bool(0.15f))
            {
                return "Paciente sano, esquema vacunal al día. Sin hallazgos patológicos relevantes.";
            }

            return $"{faker.Random.ListItem(clinicos)}. {faker.Random.ListItem(acciones)}. Programar {faker.Random.ListItem(controles)}";
        }
    }
}

