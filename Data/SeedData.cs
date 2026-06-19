using HuellitasFelices.Models;
using Microsoft.AspNetCore.Identity;

namespace HuellitasFelices.Data
{
    public static class SeedData
    {
        public static async Task Initialize(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ===== ROLES =====
            string[] roles = { "Administrador", "Supervisor", "Operador", "Consulta", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ===== USUARIO ADMINISTRADOR =====
            var adminEmail = "admin@huellitasfelices.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin1234*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
            }

            // ===== USUARIO SUPERVISOR =====
            var supervisorEmail = "supervisor@huellitasfelices.com";
            if (await userManager.FindByEmailAsync(supervisorEmail) == null)
            {
                var supervisor = new IdentityUser
                {
                    UserName = supervisorEmail,
                    Email = supervisorEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(supervisor, "Super1234*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(supervisor, "Supervisor");
                }
            }

            // ===== USUARIO OPERADOR =====
            var operadorEmail = "operador@huellitasfelices.com";
            if (await userManager.FindByEmailAsync(operadorEmail) == null)
            {
                var operador = new IdentityUser
                {
                    UserName = operadorEmail,
                    Email = operadorEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(operador, "Oper1234*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(operador, "Operador");
                }
            }

            // ===== USUARIO CONSULTA =====
            var consultaEmail = "consulta@huellitasfelices.com";
            if (await userManager.FindByEmailAsync(consultaEmail) == null)
            {
                var consulta = new IdentityUser
                {
                    UserName = consultaEmail,
                    Email = consultaEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(consulta, "Cons1234*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(consulta, "Consulta");
                }
            }

            // ===== DATOS DE PRUEBA =====

            // Empleados
            if (!context.Empleados.Any())
            {
                context.Empleados.AddRange(
                    new Empleado { Nombre = "Dr. Carlos Ramírez", Cargo = "Veterinario", Telefono = "0991234567", Salario = 1800, Activo = true },
                    new Empleado { Nombre = "Dra. Ana Torres", Cargo = "Veterinario", Telefono = "0987654321", Salario = 1800, Activo = true },
                    new Empleado { Nombre = "Luis Mendoza", Cargo = "Asistente", Telefono = "0976543210", Salario = 900, Activo = true },
                    new Empleado { Nombre = "María Suárez", Cargo = "Recepcionista", Telefono = "0965432109", Salario = 800, Activo = true },
                    new Empleado { Nombre = "Pedro Gómez", Cargo = "Asistente", Telefono = "0954321098", Salario = 900, Activo = true }
                );
                context.SaveChanges();
            }

            // Dueños
            if (!context.Duenos.Any())
            {
                context.Duenos.AddRange(
                    new Dueno { Nombre = "Juan Pérez", Telefono = "0991111111", Email = "juan@gmail.com", Direccion = "Av. Amazonas 123", Activo = true },
                    new Dueno { Nombre = "Laura Castillo", Telefono = "0992222222", Email = "laura@gmail.com", Direccion = "Calle Sucre 456", Activo = true },
                    new Dueno { Nombre = "Roberto Silva", Telefono = "0993333333", Email = "roberto@gmail.com", Direccion = "Av. 10 de Agosto 789", Activo = true },
                    new Dueno { Nombre = "Carmen Vega", Telefono = "0994444444", Email = "carmen@gmail.com", Direccion = "Calle Bolívar 321", Activo = true },
                    new Dueno { Nombre = "Diego Mora", Telefono = "0995555555", Email = "diego@gmail.com", Direccion = "Av. Colón 654", Activo = true }
                );
                context.SaveChanges();
            }

            // Mascotas
            if (!context.Mascotas.Any())
            {
                var duenos = context.Duenos.ToList();
                context.Mascotas.AddRange(
                    new Mascota { Nombre = "Max", Especie = "Perro", Raza = "Labrador", Edad = 3, Peso = 25.5m, DuenoId = duenos[0].Id, Activo = true },
                    new Mascota { Nombre = "Luna", Especie = "Gato", Raza = "Persa", Edad = 2, Peso = 4.2m, DuenoId = duenos[1].Id, Activo = true },
                    new Mascota { Nombre = "Rocky", Especie = "Perro", Raza = "Bulldog", Edad = 5, Peso = 18.0m, DuenoId = duenos[2].Id, Activo = true },
                    new Mascota { Nombre = "Mia", Especie = "Gato", Raza = "Siamés", Edad = 1, Peso = 3.5m, DuenoId = duenos[3].Id, Activo = true },
                    new Mascota { Nombre = "Toby", Especie = "Perro", Raza = "Poodle", Edad = 4, Peso = 8.0m, DuenoId = duenos[4].Id, Activo = true }
                );
                context.SaveChanges();
            }

            // Consultas
            if (!context.Consultas.Any())
            {
                var mascotas = context.Mascotas.ToList();
                context.Consultas.AddRange(
                    new Consulta { Motivo = "Vacunación anual", Diagnostico = "Mascota sana", Costo = 35.00m, MascotaId = mascotas[0].Id, Activo = true },
                    new Consulta { Motivo = "Control de peso", Diagnostico = "Peso normal", Costo = 20.00m, MascotaId = mascotas[1].Id, Activo = true },
                    new Consulta { Motivo = "Desparasitación", Diagnostico = "Tratamiento aplicado", Costo = 25.00m, MascotaId = mascotas[2].Id, Activo = true },
                    new Consulta { Motivo = "Revisión general", Diagnostico = "Todo en orden", Costo = 30.00m, MascotaId = mascotas[3].Id, Activo = true },
                    new Consulta { Motivo = "Corte de uñas", Diagnostico = "Procedimiento estético", Costo = 15.00m, MascotaId = mascotas[4].Id, Activo = true }
                );
                context.SaveChanges();
            }

            // Tratamientos
            if (!context.Tratamientos.Any())
            {
                var consultas = context.Consultas.ToList();
                context.Tratamientos.AddRange(
                    new Tratamiento { Nombre = "Vacuna Antirrábica", Descripcion = "Vacuna obligatoria anual", Costo = 15.00m, Medicamento = "Rabisin", ConsultaId = consultas[0].Id, Activo = true },
                    new Tratamiento { Nombre = "Dieta balanceada", Descripcion = "Plan alimenticio supervisado", Costo = 10.00m, Medicamento = "N/A", ConsultaId = consultas[1].Id, Activo = true },
                    new Tratamiento { Nombre = "Antiparasitario", Descripcion = "Desparasitación interna", Costo = 12.00m, Medicamento = "Drontal", ConsultaId = consultas[2].Id, Activo = true },
                    new Tratamiento { Nombre = "Vitaminas", Descripcion = "Suplemento vitamínico", Costo = 8.00m, Medicamento = "Vitovet", ConsultaId = consultas[3].Id, Activo = true },
                    new Tratamiento { Nombre = "Limpieza dental", Descripcion = "Profilaxis dental", Costo = 20.00m, Medicamento = "N/A", ConsultaId = consultas[4].Id, Activo = true }
                );
                context.SaveChanges();
            }

            // Pagos
            if (!context.Pagos.Any())
            {
                var duenos = context.Duenos.ToList();
                context.Pagos.AddRange(
                    new Pago { Monto = 35.00m, MetodoPago = "Efectivo", Estado = "Pagado", DuenoId = duenos[0].Id, Activo = true },
                    new Pago { Monto = 20.00m, MetodoPago = "Tarjeta", Estado = "Pagado", DuenoId = duenos[1].Id, Activo = true },
                    new Pago { Monto = 25.00m, MetodoPago = "Transferencia", Estado = "Pagado", DuenoId = duenos[2].Id, Activo = true },
                    new Pago { Monto = 30.00m, MetodoPago = "Efectivo", Estado = "Pendiente", DuenoId = duenos[3].Id, Activo = true },
                    new Pago { Monto = 15.00m, MetodoPago = "Tarjeta", Estado = "Pagado", DuenoId = duenos[4].Id, Activo = true }
                );
                context.SaveChanges();
            }

            // Animales en adopción
            if (!context.AnimalesAdopcion.Any())
            {
                context.AnimalesAdopcion.AddRange(
                    new AnimalAdopcion { Nombre = "Peluso", Especie = "Perro", Raza = "Mestizo", EdadAproximada = 2, Descripcion = "Muy juguetón y cariñoso", Disponible = true, Activo = true },
                    new AnimalAdopcion { Nombre = "Nube", Especie = "Gato", Raza = "Mestizo", EdadAproximada = 1, Descripcion = "Tranquilo y limpio", Disponible = true, Activo = true },
                    new AnimalAdopcion { Nombre = "Bruno", Especie = "Perro", Raza = "Mestizo", EdadAproximada = 3, Descripcion = "Ideal para familias", Disponible = true, Activo = true },
                    new AnimalAdopcion { Nombre = "Canela", Especie = "Gato", Raza = "Mestizo", EdadAproximada = 2, Descripcion = "Muy sociable", Disponible = true, Activo = true },
                    new AnimalAdopcion { Nombre = "Thor", Especie = "Perro", Raza = "Mestizo", EdadAproximada = 4, Descripcion = "Bueno con niños", Disponible = true, Activo = true }
                );
                context.SaveChanges();
            }

            // Solicitudes de adopción
            if (!context.SolicitudesAdopcion.Any())
            {
                var animales = context.AnimalesAdopcion.ToList();
                context.SolicitudesAdopcion.AddRange(
                    new SolicitudAdopcion { NombreSolicitante = "Sofía Ramos", Telefono = "0981111111", Email = "sofia@gmail.com", Estado = "Pendiente", AnimalAdopcionId = animales[0].Id, Activo = true },
                    new SolicitudAdopcion { NombreSolicitante = "Andrés Flores", Telefono = "0982222222", Email = "andres@gmail.com", Estado = "Aprobada", AnimalAdopcionId = animales[1].Id, Activo = true },
                    new SolicitudAdopcion { NombreSolicitante = "Valentina Cruz", Telefono = "0983333333", Email = "vale@gmail.com", Estado = "Pendiente", AnimalAdopcionId = animales[2].Id, Activo = true },
                    new SolicitudAdopcion { NombreSolicitante = "Mateo Herrera", Telefono = "0984444444", Email = "mateo@gmail.com", Estado = "Rechazada", AnimalAdopcionId = animales[3].Id, Activo = true },
                    new SolicitudAdopcion { NombreSolicitante = "Isabella Díaz", Telefono = "0985555555", Email = "isa@gmail.com", Estado = "Pendiente", AnimalAdopcionId = animales[4].Id, Activo = true }
                );
                context.SaveChanges();
            }
        }
    }
}