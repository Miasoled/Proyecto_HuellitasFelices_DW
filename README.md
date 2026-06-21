# HuellitasFelices

Sistema de gestión para clínica veterinaria con módulo de adopciones, desarrollado con ASP.NET Core MVC, Entity Framework Core y PostgreSQL.

---

## Descripción del proyecto

**HuellitasFelices** es una aplicación web multiplataforma que permite gestionar los procesos principales de una clínica veterinaria: registro de dueños, mascotas, consultas médicas, empleados y un módulo completo de adopciones de animales.

### Problema que resuelve
Centraliza la información de una veterinaria que anteriormente manejaba sus datos en papel o en hojas de cálculo, permitiendo un control más eficiente de pacientes, clientes y animales en adopción.

### Usuarios principales
- Veterinarios
- Asistentes
- Recepcionistas
- Administrador del sistema

---

## Módulos del sistema

| Módulo | Descripción |
|---|---|
| **Dueños** | Registro de clientes propietarios de mascotas |
| **Mascotas** | Historial de animales atendidos en la clínica |
| **Consultas** | Registro de atenciones médicas por mascota |
| **Empleados** | Gestión del personal de la clínica |
| **Adopciones** | Animales disponibles para ser adoptados |
| **Solicitudes** | Solicitudes de adopción recibidas |

---

## Tecnologías utilizadas

- [.NET 10](https://dotnet.microsoft.com/) — Framework principal
- [ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/mvc) — Patrón de arquitectura
- [Entity Framework Core 10](https://docs.microsoft.com/ef/core/) — ORM para acceso a datos
- [PostgreSQL](https://www.postgresql.org/) — Motor de base de datos
- [Npgsql](https://www.npgsql.org/) — Proveedor EF Core para PostgreSQL
- [Bootstrap 5](https://getbootstrap.com/) — Estilos y diseño responsive

---

## Estructura de la base de datos

```
Duenos (1) ──────── (N) Mascotas
Mascotas (1) ─────── (N) Consultas
AnimalesAdopcion (1) ─ (N) SolicitudesAdopcion
```

### Tablas principales

| Tabla | Descripción |
|---|---|
| `Duenos` | Propietarios de mascotas |
| `Mascotas` | Animales registrados como pacientes |
| `Consultas` | Atenciones médicas realizadas |
| `Empleados` | Personal de la clínica |
| `AnimalesAdopcion` | Animales disponibles para adoptar |
| `SolicitudesAdopcion` | Solicitudes de adopción recibidas |

---

## Estructura del proyecto

```
HuellitasFelices/
├── Controllers/          # Controladores MVC
├── Data/                 # DbContext y Seeder
├── Models/               # Entidades del negocio
├── Views/                # Interfaces Razor (.cshtml)
├── Services/             # Lógica de apoyo
├── ViewModels/           # Datos preparados para vistas
├── Migrations/           # Migraciones de EF Core
├── wwwroot/              # Archivos estáticos (CSS, JS)
├── appsettings.json      # Configuración (no subir con clave real)
├── appsettings.example.json  # Ejemplo de configuración
└── Program.cs            # Punto de entrada y registro de servicios
```

---

## Instrucciones de ejecución

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Visual Studio Code](https://code.visualstudio.com/)
- Entity Framework CLI:
```bash
dotnet tool install --global dotnet-ef
```

### Pasos para ejecutar

**1. Clonar el repositorio**
```bash
git clone https://github.com/TU_USUARIO/HuellitasFelices.git
cd HuellitasFelices
```

**2. Crear la base de datos en PostgreSQL**
```sql
CREATE USER huellitas_user WITH PASSWORD 'tu_clave';
CREATE DATABASE huellitas_felices WITH OWNER = huellitas_user;
GRANT ALL ON SCHEMA public TO huellitas_user;
ALTER SCHEMA public OWNER TO huellitas_user;
```

**3. Configurar la cadena de conexión**

Copia el archivo de ejemplo y configura tu clave:
```bash
cp appsettings.example.json appsettings.json
```

Edita `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=huellitas_felices;Username=huellitas_user;Password=TU_CLAVE"
  }
}
```

**4. Ejecutar migraciones**
```bash
dotnet ef database update
```

**5. Ejecutar la aplicación**
```bash
dotnet run
```

**6. Abrir en el navegador**
```
http://localhost:5248
```

> Al iniciar por primera vez, el sistema cargará automáticamente 5 registros de prueba en cada tabla.

---

## Paquetes NuGet instalados

| Paquete | Versión | Función |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.x | Conexión con PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` | 10.x | Herramientas de diseño para migraciones |
| `Microsoft.EntityFrameworkCore.Tools` | 10.x | Comandos dotnet ef |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | 10.x | Scaffolding de CRUDs |

---

## Preparado para la fase final

El proyecto está diseñado para escalar en la siguiente fase:

| Aspecto | Implementación futura |
|---|---|
| **500.000 registros** | Índices en campos de búsqueda frecuente |
| **Paginación** | Evitar cargar todos los registros en memoria |
| **Filtros** | Búsqueda por nombre, especie, estado |
| **Eliminación lógica** | Campo `Activo` en todas las entidades |
| **Sesión** | Autenticación para proteger módulos internos |
| **Reportes** | Estadísticas de consultas, adopciones y ventas |


## Diagrama de la base de datos 
<img width="1380" height="1471" alt="huellitasFelices det pgerd" src="https://github.com/user-attachments/assets/b9eec9b0-996b-4a31-8c38-196250e23c22" />

