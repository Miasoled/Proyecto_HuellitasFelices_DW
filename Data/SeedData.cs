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
            string[] roles = { "Administrador", "Doctor", "Cliente" };
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

            // ===== USUARIOS DOCTORES =====
            var doctorAccounts = new (string Email, string Password)[]
            {
                ("doctor@huellitasfelices.com", "Doctor1234*"),
                ("doctora.ana@huellitasfelices.com", "Doctor1234*"),
                ("doctor.luis@huellitasfelices.com", "Doctor1234*")
            };

            foreach (var (email, password) in doctorAccounts)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");
                    }
                }
            }

            // ===== DATOS DE PRUEBA =====

            // Empleados
            if (!context.Empleados.Any())
            {
                context.Empleados.AddRange(
                    new Empleado { Nombre = "Dr. Carlos Ramírez", Cargo = "Veterinario", Email = "doctor@huellitasfelices.com", Telefono = "0991234567", Salario = 1800, Activo = true },
                    new Empleado { Nombre = "Dra. Ana Torres", Cargo = "Veterinario", Email = "doctora.ana@huellitasfelices.com", Telefono = "0987654321", Salario = 1800, Activo = true },
                    new Empleado { Nombre = "Dr. Luis Mendoza", Cargo = "Veterinario", Email = "doctor.luis@huellitasfelices.com", Telefono = "0976543210", Salario = 1800, Activo = true },
                    new Empleado { Nombre = "María Suárez", Cargo = "Recepcionista", Email = "maria.suarez@huellitasfelices.com", Telefono = "0965432109", Salario = 800, Activo = true },
                    new Empleado { Nombre = "Pedro Gómez", Cargo = "Asistente", Email = "pedro.gomez@huellitasfelices.com", Telefono = "0954321098", Salario = 900, Activo = true }
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
                    new Mascota { Nombre = "Max", Especie = "Perro", Raza = "Labrador", Sexo = "Macho", FechaNacimiento = DateTime.UtcNow.AddYears(-3), Peso = 25.5m, DuenoId = duenos[0].Id, Activo = true },
                    new Mascota { Nombre = "Luna", Especie = "Gato", Raza = "Persa", Sexo = "Hembra", FechaNacimiento = DateTime.UtcNow.AddYears(-2), Peso = 4.2m, DuenoId = duenos[1].Id, Activo = true },
                    new Mascota { Nombre = "Rocky", Especie = "Perro", Raza = "Bulldog", Sexo = "Macho", FechaNacimiento = DateTime.UtcNow.AddYears(-5), Peso = 18.0m, DuenoId = duenos[2].Id, Activo = true },
                    new Mascota { Nombre = "Mia", Especie = "Gato", Raza = "Siamés", Sexo = "Hembra", FechaNacimiento = DateTime.UtcNow.AddYears(-1), Peso = 3.5m, DuenoId = duenos[3].Id, Activo = true },
                    new Mascota { Nombre = "Toby", Especie = "Perro", Raza = "Poodle", Sexo = "Macho", FechaNacimiento = DateTime.UtcNow.AddYears(-4), Peso = 8.0m, DuenoId = duenos[4].Id, Activo = true }
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

            // ===== CATEGORÍAS =====
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Alimento", Descripcion = "Alimentos balanceados para mascotas", Activo = true },
                    new Categoria { Nombre = "Medicamento", Descripcion = "Medicamentos veterinarios", Activo = true },
                    new Categoria { Nombre = "Accesorio", Descripcion = "Collares, juguetes, camas y más", Activo = true },
                    new Categoria { Nombre = "Higiene", Descripcion = "Shampoo, cepillos, limpieza dental", Activo = true }
                );
                context.SaveChanges();
            }

            // ===== PROVEEDORES =====
            if (!context.Proveedores.Any())
            {
                context.Proveedores.AddRange(
                    new Proveedor { Nombre = "PetSupply Ecuador", Telefono = "022345678", Email = "ventas@petsupply.ec", Direccion = "Av. De los Shyris N36-40, Quito", Activo = true },
                    new Proveedor { Nombre = "VetFarma", Telefono = "0998765432", Email = "info@vetfarma.ec", Direccion = "Calle Colón 1225, Quito", Activo = true },
                    new Proveedor { Nombre = "MascotaTotal", Telefono = "0987123456", Email = "compras@mascotatotal.ec", Direccion = "Av. Eloy Alfaro 1234, Quito", Activo = true }
                );
                context.SaveChanges();
            }

            // ===== PRODUCTOS =====
            if (!context.Productos.Any())
            {
                var cats = context.Categorias.ToList();
                var provs = context.Proveedores.ToList();

                var catAlimento = cats.First(c => c.Nombre == "Alimento");
                var catMedicamento = cats.First(c => c.Nombre == "Medicamento");
                var catAccesorio = cats.First(c => c.Nombre == "Accesorio");
                var catHigiene = cats.First(c => c.Nombre == "Higiene");
                var provPS = provs.First(p => p.Nombre == "PetSupply Ecuador");
                var provVF = provs.First(p => p.Nombre == "VetFarma");
                var provMT = provs.First(p => p.Nombre == "MascotaTotal");

                context.Productos.AddRange(
                    // Alimentos
                    new Producto { Nombre = "Dog Chow Adulto 15kg", Descripcion = "Alimento balanceado para perros adultos de tamaño mediano y grande. Fórmula con proteína de pollo.", PrecioCompra = 28.50m, PrecioVenta = 38.99m, CodigoBarras = "7501020300123", UnidadMedida = "Paquete", StockMinimo = 10, CategoriaId = catAlimento.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Cat Chow Gatos Adultos 8kg", Descripcion = "Nutrición completa para gatos adultos. Con taurina y ácidos grasos esenciales.", PrecioCompra = 22.00m, PrecioVenta = 31.50m, CodigoBarras = "7501020300124", UnidadMedida = "Paquete", StockMinimo = 8, CategoriaId = catAlimento.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Dog Chow Cachorros 3kg", Descripcion = "Alimento especial para cachorros de razas pequeñas y medianas. Rico en calcio y fósforo.", PrecioCompra = 10.50m, PrecioVenta = 16.99m, CodigoBarras = "7501020300125", UnidadMedida = "Paquete", StockMinimo = 12, CategoriaId = catAlimento.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Cat Chow Gatitos 2kg", Descripcion = "Alimento para gatitos en crecimiento. Texturas suaves y fáciles de masticar.", PrecioCompra = 7.80m, PrecioVenta = 12.99m, CodigoBarras = "7501020300126", UnidadMedida = "Paquete", StockMinimo = 10, CategoriaId = catAlimento.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Dog Chow Light 12kg", Descripcion = "Alimento reducido en grasa para perros con tendencia a sobrepeso. Con L-carnitina.", PrecioCompra = 30.00m, PrecioVenta = 42.50m, CodigoBarras = "7501020300127", UnidadMedida = "Paquete", StockMinimo = 6, CategoriaId = catAlimento.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Pienso Premium Carne y Arroz 10kg", Descripcion = "Alimento premium con trozos de carne real y arroz integral. Sin colorantes artificiales.", PrecioCompra = 35.00m, PrecioVenta = 49.99m, CodigoBarras = "7501020300128", UnidadMedida = "Paquete", StockMinimo = 5, CategoriaId = catAlimento.Id, ProveedorId = provMT.Id, Activo = true },

                    // Medicamentos
                    new Producto { Nombre = "Antiparasitario Drontal Plus", Descripcion = "Desparasitante interno para perros. Tratamiento contra tenias, gusitos y ascarias.", PrecioCompra = 5.50m, PrecioVenta = 9.99m, CodigoBarras = "7501020300201", UnidadMedida = "Unidad", StockMinimo = 20, CategoriaId = catMedicamento.Id, ProveedorId = provVF.Id, Activo = true },
                    new Producto { Nombre = "Vacuna Antirrábica Rabisin", Descripcion = "Vacuna antirrábica para perros y gatos. Protección anual obligatoria.", PrecioCompra = 4.00m, PrecioVenta = 12.00m, CodigoBarras = "7501020300202", UnidadMedida = "Unidad", StockMinimo = 15, CategoriaId = catMedicamento.Id, ProveedorId = provVF.Id, Activo = true },
                    new Producto { Nombre = "Gotas para Ojos VetOptic", Descripcion = "Solución limpiadora y protectora para los ojos de mascotas. Alivia irritaciones.", PrecioCompra = 3.20m, PrecioVenta = 7.50m, CodigoBarras = "7501020300203", UnidadMedida = "Unidad", StockMinimo = 15, CategoriaId = catMedicamento.Id, ProveedorId = provVF.Id, Activo = true },
                    new Producto { Nombre = "Suplemento Vitamínico Vitovet", Descripcion = "Complex vitamínico para fortalecer el sistema inmunológico. Ideal para mascotas convalecientes.", PrecioCompra = 4.50m, PrecioVenta = 8.99m, CodigoBarras = "7501020300204", UnidadMedida = "Unidad", StockMinimo = 12, CategoriaId = catMedicamento.Id, ProveedorId = provVF.Id, Activo = true },

                    // Accesorios
                    new Producto { Nombre = "Collar de Cuero Ajustable", Descripcion = "Collar de cuero genuino con hebilla de seguridad. Disponible en varios tamaños.", PrecioCompra = 6.00m, PrecioVenta = 14.99m, CodigoBarras = "7501020300301", UnidadMedida = "Unidad", StockMinimo = 10, CategoriaId = catAccesorio.Id, ProveedorId = provMT.Id, Activo = true },
                    new Producto { Nombre = "Juguete Pelota Interactiva", Descripcion = "Pelota de goma resistente con dispensador de snacks. Perfecta para juego activo.", PrecioCompra = 3.00m, PrecioVenta = 8.50m, CodigoBarras = "7501020300302", UnidadMedida = "Unidad", StockMinimo = 15, CategoriaId = catAccesorio.Id, ProveedorId = provMT.Id, Activo = true },
                    new Producto { Nombre = "Cama Orthopédica para Perro", Descripcion = "Cama con espuma de memoria ortopédica. Ideal para perros mayores o con problemas articulares.", PrecioCompra = 25.00m, PrecioVenta = 45.99m, CodigoBarras = "7501020300303", UnidadMedida = "Unidad", StockMinimo = 5, CategoriaId = catAccesorio.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Correa Retráctil 5 metros", Descripcion = "Correa extensible con freno automático y empuñadura ergonómica. Soporta hasta 15kg.", PrecioCompra = 7.00m, PrecioVenta = 15.99m, CodigoBarras = "7501020300304", UnidadMedida = "Unidad", StockMinimo = 10, CategoriaId = catAccesorio.Id, ProveedorId = provMT.Id, Activo = true },

                    // Higiene
                    new Producto { Nombre = "Shampoo Antipulgas", Descripcion = "Shampoo medicado con extracto de neem. Elimina pulgas y garrapatas. Apto para perros y gatos.", PrecioCompra = 4.50m, PrecioVenta = 10.99m, CodigoBarras = "7501020300401", UnidadMedida = "Litro", StockMinimo = 15, CategoriaId = catHigiene.Id, ProveedorId = provVF.Id, Activo = true },
                    new Producto { Nombre = "Pasta Dental Veterinaria", Descripcion = "Pasta dental enzimática para mascotas. Sabor a pollo. Uso diario recomendado.", PrecioCompra = 3.80m, PrecioVenta = 8.99m, CodigoBarras = "7501020300402", UnidadMedida = "Unidad", StockMinimo = 20, CategoriaId = catHigiene.Id, ProveedorId = provVF.Id, Activo = true },
                    new Producto { Nombre = "Cepillo Doble Cara para Perro", Descripcion = "Cepillo con cerdas suaves y duras. Elimina pelo muerto y distribute los aceites naturales.", PrecioCompra = 2.50m, PrecioVenta = 6.50m, CodigoBarras = "7501020300403", UnidadMedida = "Unidad", StockMinimo = 12, CategoriaId = catHigiene.Id, ProveedorId = provPS.Id, Activo = true },
                    new Producto { Nombre = "Toallas Húmedas para Mascotas", Descripcion = "Paquete de 100 toallas húmedas con aloe vera. Para limpieza diaria de patas y pelaje.", PrecioCompra = 3.00m, PrecioVenta = 6.99m, CodigoBarras = "7501020300404", UnidadMedida = "Paquete", StockMinimo = 20, CategoriaId = catHigiene.Id, ProveedorId = provPS.Id, Activo = true }
                );
                context.SaveChanges();
            }

            // ===== INVENTARIOS (stock para cada producto) =====
            if (!context.Inventarios.Any())
            {
                var productos = context.Productos.ToList();
                var random = new Random(42);
                foreach (var prod in productos)
                {
                    context.Inventarios.Add(new Inventario
                    {
                        ProductoId = prod.Id,
                        StockActual = random.Next(5, 50),
                        FechaActualizacion = DateTime.UtcNow
                    });
                }
                context.SaveChanges();
            }
        }
    }
}