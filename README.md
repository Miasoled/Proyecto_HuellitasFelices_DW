# Huellitas Felices

Sistema web empresarial para la gestión integral de una clínica veterinaria, desarrollado con ASP.NET Core MVC, Entity Framework Core y PostgreSQL.

Huellitas Felices centraliza la administración clínica, comercial y operativa de una veterinaria: clientes, mascotas, consultas, tratamientos, adopciones, productos, compras, inventario por sucursal, ventas, pagos, seguridad, auditoría, correo electrónico, inteligencia artificial y reportes.

## Funcionalidades principales

### Gestión veterinaria

- Registro y administración de dueños.
- Expedientes de mascotas.
- Consultas médicas y tratamientos.
- Asignación de veterinarios.
- Gestión de empleados y sucursales.
- Animales disponibles para adopción.
- Seguimiento de solicitudes de adopción.

### Tienda, ventas e inventario

- Catálogo de productos por categoría.
- Proveedores y compras de mercadería.
- Inventario independiente por sucursal.
- Visualización del stock total, stock de la sede principal y existencias de las demás sedes.
- Carrito de compras.
- Ventas y facturación.
- Reserva temporal de productos durante el pago.
- Movimientos de compra, venta, ajuste, devolución y transferencia.
- Buscador de productos en el formulario de transferencias entre sucursales.
- Liberación automática de reservas cuando el pago falla, se cancela o expira.
- Control de concurrencia para reducir el riesgo de inventario negativo.

### Pagos

- PayPal Sandbox.
- PayPhone.
- Abstracción común mediante `IPaymentGateway`.
- Creación, verificación y cancelación de pagos.
- Validación del monto confirmado por la pasarela.
- Procesamiento idempotente de confirmaciones.
- Registro de estados internos y externos.
- Reporte filtrable de transacciones.

> Todas las transacciones económicas deben realizarse en entornos Sandbox o de prueba.

### Seguridad y cuentas

- ASP.NET Core Identity.
- Confirmación obligatoria del correo.
- Recuperación y cambio de contraseña.
- Bloqueo temporal después de cinco intentos fallidos.
- Autenticación multifactor TOTP.
- Código QR compatible con aplicaciones autenticadoras.
- Códigos de recuperación.
- Autenticación externa con Google, cuando se configura.
- Roles y autorización por módulo.
- Página `AccessDenied`.
- Auditoría de accesos y operaciones críticas.

### Correo electrónico

- Confirmación de cuenta.
- Recuperación de contraseña.
- Notificación de cambio de contraseña.
- Aviso de cuenta bloqueada.
- Confirmación de activación MFA.
- Confirmación de venta.
- Notificación de pago fallido.
- Aviso de inventario crítico.
- Notificaciones de adopción.
- Cola persistida en `EmailLogs`.
- Procesamiento asíncrono mediante `EmailWorker`.

### Inteligencia artificial

- Integración con Ollama mediante API HTTP.
- Modelo predeterminado `qwen2.5:0.5b`.
- Asistente informativo contextualizado con datos de la veterinaria.
- Consultas sobre clientes, mascotas, consultas, inventario, ventas, compras, tratamientos y adopciones.
- Consulta del historial clínico de una mascota cuando su nombre aparece en la pregunta.
- Recuperación de sus cinco consultas más recientes, diagnósticos y tratamientos.
- Resumen de las cinco ventas pagadas más recientes con fecha, total y productos.
- Respuestas orientativas sobre síntomas y cuidados generales, con señales de alarma y recomendación veterinaria.
- Control de tiempo máximo de espera.
- Manejo de errores cuando Ollama no está disponible.

La inteligencia artificial funciona únicamente como apoyo informativo. No sustituye un diagnóstico veterinario. Las decisiones sobre pagos, precios, permisos, inventario y eliminación de información permanecen bajo reglas programadas.

### Reportes

El sistema incluye reportes de:

1. Consultas.
2. Mascotas.
3. Dueños.
4. Empleados.
5. Adopciones.
6. Servicios.
7. Inventario.
8. Auditoría.
9. Pagos.
10. Ventas.
11. Productos más vendidos.
12. Productos con bajo inventario.
13. Clientes con mayor actividad de compra.
14. Usuarios con MFA.
15. Accesos fallidos.

Los reportes emplean filtros, agrupaciones, proyecciones y paginación ejecutada principalmente desde PostgreSQL.

## Tecnologías

| Componente | Tecnología |
|---|---|
| Framework | .NET 10 |
| Backend | ASP.NET Core MVC |
| Lenguaje | C# |
| ORM | Entity Framework Core 10 |
| Base de datos | PostgreSQL |
| Proveedor EF Core | Npgsql |
| Identidad | ASP.NET Core Identity |
| Interfaz | Razor Views y Bootstrap 5 |
| Correo | MailKit y SMTP |
| Pagos | PayPal Sandbox y PayPhone |
| IA | Ollama y `qwen2.5:0.5b` |
| Datos masivos | Bogus |
| Caché y sesión | Redis |
| Contenedores | Docker |
| Orquestación | Docker Swarm |

## Arquitectura

La solución utiliza el patrón Modelo-Vista-Controlador y servicios especializados:

```text
Navegador
   |
   v
Controllers / Razor Pages
   |
   v
Servicios de negocio
   |-- IAccountService
   |-- IAuditService
   |-- IEmailService
   |-- IInventoryService
   |-- IPaymentService
   |-- IPaymentGateway
   |-- IAIService
   |-- IContextProviderService
   |
   v
Entity Framework Core
   |
   v
PostgreSQL
```

Las integraciones externas —SMTP, PayPal, PayPhone y Ollama— se encuentran encapsuladas en servicios para evitar que los controladores dependan directamente de ellas.

## Estructura del repositorio

```text
HuellitasFelices/
├── Areas/Identity/          # Registro, acceso, recuperación y MFA
├── Controllers/            # Controladores MVC
├── Data/                   # DbContext y datos iniciales
├── Migrations/             # Migraciones de Entity Framework Core
├── Models/                 # Entidades y modelos de presentación
├── Services/               # Lógica de negocio e integraciones
│   └── PaymentGateway/     # PayPal, PayPhone y contrato común
├── Settings/               # Configuración tipada
├── Views/                  # Vistas Razor
├── wwwroot/                # CSS, JavaScript, imágenes y bibliotecas
├── Program.cs              # Registro de servicios y pipeline HTTP
├── Dockerfile              # Construcción de la imagen
├── compose.yaml            # Entorno local con Docker Compose
├── docker-stack.yml        # Despliegue con Docker Swarm
└── docker-entrypoint.sh    # Lectura de Docker Secrets
```

## Modelo de datos

Entidades principales:

- `Dueno`, `Mascota`, `Consulta` y `Tratamiento`.
- `Empleado` y `Sucursal`.
- `AnimalAdopcion` y `SolicitudAdopcion`.
- `Categoria`, `Proveedor` y `Producto`.
- `Compra` y `DetalleCompra`.
- `Inventario` y `MovimientoInventario`.
- `Venta` y `DetalleVenta`.
- `Pago`.
- `EmailLog`.
- `AuditLog`.

Relaciones generales:

```text
Dueno 1 ─── N Mascota
Mascota 1 ─── N Consulta
Consulta 1 ─── N Tratamiento
Consulta 1 ─── N ConsultaMedicamento

Categoria 1 ─── N Producto
Proveedor 1 ─── N Producto
Producto 1 ─── N Inventario
Sucursal 1 ─── N Inventario

Compra 1 ─── N DetalleCompra
Venta 1 ─── N DetalleVenta
Venta 1 ─── N Pago

AnimalAdopcion 1 ─── N SolicitudAdopcion
```

Diagrama histórico del modelo:

<img width="1380" height="1471" alt="Diagrama de base de datos de Huellitas Felices" src="https://github.com/user-attachments/assets/b9eec9b0-996b-4a31-8c38-196250e23c22" />

## Optimización y paginación

Los listados masivos emplean consultas ejecutadas desde PostgreSQL:

```csharp
var query = _context.Productos
    .AsNoTracking()
    .Where(p => p.Activo);

if (!string.IsNullOrWhiteSpace(busqueda))
{
    query = query.Where(p =>
        EF.Functions.ILike(p.Nombre, $"%{busqueda}%"));
}

var total = await query.CountAsync();

var productos = await query
    .OrderBy(p => p.Nombre)
    .Skip((pagina - 1) * tamanioPagina)
    .Take(tamanioPagina)
    .ToListAsync();
```

Se configuraron índices para campos utilizados en búsquedas, relaciones y reportes, incluyendo:

- Nombre y correo de clientes.
- Nombre y dueño de mascotas.
- Estado y fecha de consultas.
- Nombre, código de barras y categoría de productos.
- Producto, fecha y tipo de movimientos.
- Estado, proveedor, venta e identificador externo de pagos.
- Acción, entidad, usuario y fecha de auditorías.
- Índice único compuesto de producto y sucursal en inventario.

## Inventario transaccional

`IInventoryService` concentra las operaciones de inventario:

```csharp
public interface IInventoryService
{
    Task<Inventario?> GetStockAsync(int productoId);
    Task<int> GetTotalStockAsync(int productoId);

    Task<MovimientoInventario> RegistrarCompraAsync(
        int compraId, int productoId, int cantidad,
        string? usuarioId, int? sucursalId);

    Task<MovimientoInventario?> RegistrarVentaAsync(
        int productoId, int cantidad, string? usuarioId,
        string? referencia, string? observacion);

    Task<MovimientoInventario?> AjustarAsync(
        int productoId, int? sucursalId, int nuevoStock,
        string? usuarioId, string? motivo);

    Task<bool> RegistrarDevolucionAsync(
        int productoId, int sucursalId, int cantidad,
        int? ventaId, string? usuarioId, string? motivo);

    Task<List<MovimientoInventario>> GetMovimientosAsync(
        int? productoId = null, DateTime? desde = null,
        DateTime? hasta = null, int pagina = 1,
        int tamanioPagina = 20);

    Task<int> GetTotalMovimientosAsync(
        int? productoId = null, DateTime? desde = null,
        DateTime? hasta = null);

    Task<List<MovimientoInventario>> ReservarStockParaVentaAsync(
        int ventaId,
        List<(int ProductoId, int Cantidad)> items,
        string? usuarioId);

    Task RevertirReservaAsync(
        int ventaId, string? usuarioId, string motivo);

    Task<bool> TransferirStockAsync(
        int productoId, int sucursalOrigenId,
        int sucursalDestinoId, int cantidad,
        string? usuarioId, string? observacion);
}
```

Las modificaciones relacionadas se ejecutan mediante transacciones:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

try
{
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

La entidad `Inventario` incluye `RowVersion`, y el servicio controla `DbUpdateConcurrencyException`.

El listado de inventario carga la sucursal asociada a cada existencia y presenta por separado:

- Stock acumulado de todas las sucursales.
- Stock disponible en la sede principal.
- Stock disponible en cada sede secundaria.
- Alerta visual cuando el total alcanza o baja del mínimo configurado.

Las transferencias validan producto, sucursal de origen, sucursal de destino y cantidad. El formulario incorpora un filtro por nombre para localizar productos sin recorrer manualmente toda la lista.

## Flujo de pago

1. El cliente crea una venta pendiente.
2. El sistema valida y reserva el inventario.
3. Se crea un registro `Pago`.
4. El cliente selecciona PayPal o PayPhone.
5. La pasarela procesa la operación en su entorno de prueba.
6. El backend verifica identificador, estado y monto.
7. Si el pago es aprobado, la venta cambia a `Pagada`.
8. Se guardan los detalles y se registra la auditoría.
9. Se envía el correo de confirmación.
10. Si el pago falla, se cancela o expira, se libera la reserva.

La confirmación es idempotente:

```csharp
if (pago.Estado == "Aprobado")
{
    return pago;
}
```

Además, el registro se recarga dentro de la transacción para detectar confirmaciones concurrentes.

## Roles

El seeder crea los siguientes roles:

- `Administrador`
- `Doctor`
- `Cliente`
- `Supervisor`
- `Operador`
- `Auditor`
- `Consulta`

Ejemplo de protección:

```csharp
[Authorize(Roles = "Administrador,Supervisor,Operador")]
public class InventarioController : Controller
{
}
```

## Configuración local

### Requisitos

- .NET 10 SDK.
- PostgreSQL 15 o superior.
- Git.
- Entity Framework CLI.
- Opcional: Docker Desktop.
- Opcional: Ollama.

Instalar las herramientas de Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clonar

```bash
git clone https://github.com/Miasoled/Proyecto_HuellitasFelices_DW.git
cd Proyecto_HuellitasFelices_DW
```

### 2. Crear PostgreSQL

```sql
CREATE USER huellitas_user WITH PASSWORD 'CAMBIAR_ESTA_CLAVE';
CREATE DATABASE huellitas_felices WITH OWNER = huellitas_user;
GRANT ALL ON SCHEMA public TO huellitas_user;
ALTER SCHEMA public OWNER TO huellitas_user;
```

### 3. Configurar secretos de desarrollo

No guardes credenciales reales en `appsettings.json`. Utiliza User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=huellitas_felices;Username=huellitas_user;Password=CAMBIAR_ESTA_CLAVE"
dotnet user-secrets set "EmailSettings:SenderEmail" "correo@example.com"
dotnet user-secrets set "EmailSettings:Password" "CLAVE_SMTP"
dotnet user-secrets set "PaymentSettings:PayPal:ClientId" "CLIENT_ID_SANDBOX"
dotnet user-secrets set "PaymentSettings:PayPal:ClientSecret" "CLIENT_SECRET_SANDBOX"
dotnet user-secrets set "PayPhone:Token" "TOKEN_DE_PRUEBA"
dotnet user-secrets set "PayPhone:StoreId" "STORE_ID"
dotnet user-secrets set "Authentication:Google:ClientId" "CLIENT_ID_GOOGLE"
dotnet user-secrets set "Authentication:Google:ClientSecret" "CLIENT_SECRET_GOOGLE"
```

Las credenciales de Google son opcionales. Las secciones y nombres exactos pueden consultarse en las clases de `Settings/` y en `Program.cs`.

### 4. Aplicar migraciones

```bash
dotnet ef database update
```

### 5. Ejecutar

```bash
dotnet run
```

La URL local se muestra en la consola y también puede consultarse en `Properties/launchSettings.json`.

### 6. Verificar estado

```text
/health
```

## Configuración de Ollama

Instala Ollama y descarga el modelo:

```bash
ollama pull qwen2.5:0.5b
ollama serve
```

Configuración predeterminada:

```json
{
  "AI": {
    "BaseUrl": "http://localhost:11434",
    "ModelName": "qwen2.5:0.5b",
    "TimeoutSeconds": 30
  }
}
```

## Docker Compose

El entorno local contenedorizado incluye la aplicación y PostgreSQL 17. Las migraciones y los datos iniciales se aplican automáticamente, mientras que la carga masiva permanece desactivada de forma predeterminada. Las claves de Data Protection se conservan en un volumen para mantener válidas las sesiones y los tokens después de reiniciar el contenedor.

Antes de iniciar, crea un archivo `.env` local con las variables requeridas. No subas ese archivo al repositorio.

```bash
docker compose up --build -d
docker compose logs -f web
```

Aplicación:

```text
http://localhost:8081
```

Detener sin eliminar los volúmenes:

```bash
docker compose down
```

## Docker Swarm

La pila incluye:

| Servicio | Función | Réplicas iniciales |
|---|---|---:|
| `postgres` | Base de datos persistente | 1 |
| `redis` | Sesión y Data Protection | 1 |
| `migrate` | Migraciones y seeder | 1 |
| `web` | Aplicación ASP.NET Core | 2 |
| `email-worker` | Procesamiento de correos | 1 |
| `ollama` | Modelo de IA | 1 |
| `ollama-init` | Descarga inicial del modelo | 1 |

La pila utiliza redes overlay, volúmenes, health checks y Docker Secrets.

La aplicación desplegada por Swarm queda disponible en:

```text
http://localhost:8090
```

El puerto `8081` pertenece únicamente al entorno local de Docker Compose.

La variable `APP_ROLE` separa las responsabilidades de cada contenedor:

- `migrate`: aplica migraciones, ejecuta el seeder y finaliza.
- `web`: atiende solicitudes HTTP sin ejecutar trabajos en segundo plano.
- `worker`: procesa la cola de correos y la expiración de pagos pendientes.
- `all`: comportamiento predeterminado para ejecución local, con web y workers en un mismo proceso.

Redis comparte la sesión distribuida y las claves de Data Protection entre las réplicas web. Esto permite escalar el servicio conservando sesiones, autenticación y MFA.

### Crear secretos

Ejemplo para PostgreSQL:

```bash
printf "CLAVE_SEGURA" | docker secret create huellitas_postgres_password -
```

Crea de la misma manera:

- `huellitas_email_password`
- `huellitas_email_sender_email`
- `huellitas_paypal_client_id`
- `huellitas_paypal_client_secret`
- `huellitas_payphone_token`
- `huellitas_payphone_store_id`

### Desplegar

```bash
docker swarm init
docker node ls
docker build -t huellitasfelices:latest .
docker stack deploy -c docker-stack.yml huellitas
docker stack services huellitas
docker stack ps huellitas
```

Si la aplicación se publicará con otra dirección, define `PUBLIC_URL` antes del despliegue para construir correctamente las URL de retorno de PayPal:

```bash
export PUBLIC_URL=https://veterinaria.example.com
docker stack deploy -c docker-stack.yml huellitas
```

Escalar la aplicación:

```bash
docker service scale huellitas_web=3
```

Eliminar la pila:

```bash
docker stack rm huellitas
```

## Carga masiva

La generación se habilita mediante configuración:

```text
Database__Seed=true
Database__GenerateMassData=true
```

`CargaMasivaService` utiliza Bogus, inserciones por lotes y limpieza del `ChangeTracker`.

### Advertencia sobre el conteo

Las metas actuales declaradas en el generador suman **1.062.000 registros**, aunque el comentario del código indica 1.000.000. Antes de utilizar esta funcionalidad como evidencia académica se debe:

1. Corregir la distribución para obtener exactamente 1.000.000.
2. Definir las tablas incluidas y excluidas.
3. Ejecutar una consulta SQL de comprobación.
4. Guardar la captura y el tiempo de generación.

Por este motivo, el repositorio demuestra que existe carga masiva, pero todavía no debe afirmarse que el millón exacto está validado.

## Eliminación lógica y trazabilidad

Las entidades maestras utilizan:

```csharp
public bool Activo { get; set; } = true;
public DateTime? FechaEliminacion { get; set; }
public string? EliminadoPor { get; set; }
```

El contexto aplica filtros globales a dueños, mascotas, consultas, tratamientos, empleados, adopciones, categorías, proveedores, ventas y sucursales.

Los pagos, movimientos de inventario y auditorías se conservan como historial transaccional.

## Compilación y pruebas

Compilar:

```bash
dotnet build
```

En la revisión más reciente la solución compiló con:

```text
0 errores
2 advertencias
```

Las advertencias corresponden a vulnerabilidades conocidas de gravedad baja en versiones transitivas de `NuGet.Packaging` y `NuGet.Protocol`.

Actualmente no existe un proyecto de pruebas automatizadas en el repositorio. Antes de una entrega formal se recomienda cubrir, como mínimo:

- Registro y confirmación de usuario.
- Recuperación de contraseña.
- Activación y acceso con MFA.
- Bloqueo por credenciales incorrectas.
- Acceso sin permisos.
- Compra, reserva, venta, devolución y transferencia.
- Venta rechazada por stock insuficiente.
- Pago aprobado, cancelado y fallido en cada pasarela.
- Confirmación duplicada.
- Consulta paginada.
- IA disponible y detenida.
- Envío de correo.
- Caída de una réplica web.

## Variables y secretos

Nunca confirmes en Git:

- Contraseñas de PostgreSQL.
- Credenciales SMTP.
- Client ID o Client Secret de PayPal.
- Token o Store ID de PayPhone.
- Credenciales OAuth de Google.
- Archivos `.env`.

Utiliza:

- User Secrets durante el desarrollo.
- Variables de entorno en contenedores.
- Docker Secrets en Swarm.

## Estado del proyecto

La aplicación incluye las funciones principales requeridas para el tercer parcial: seguridad, MFA, pagos, inventario transaccional, correo, auditoría, reportes, IA y despliegue distribuido.

Antes de presentar el proyecto deben completarse las evidencias de ejecución y corregirse el conteo de la carga masiva para que alcance exactamente el valor establecido.

## Licencia

Proyecto académico. No se ha definido una licencia pública de distribución.
