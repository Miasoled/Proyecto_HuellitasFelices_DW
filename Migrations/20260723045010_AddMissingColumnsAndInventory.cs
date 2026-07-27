using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Tratamientos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "SolicitudesAdopcion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Pagos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Mascotas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Empleados",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Duenos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "Consultas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EliminadoPor",
                table: "AnimalesAdopcion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DireccionIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdentificadorEntidad = table.Column<int>(type: "integer", nullable: true),
                    ValorAnterior = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValorNuevo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Nivel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RUC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric", nullable: false),
                    CodigoBarras = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnidadMedida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    ProveedorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCompra = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false),
                    SucursalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockActual = table.Column<int>(type: "integer", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    SucursalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventarios_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventarios_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false),
                    CompraId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoMovimiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    StockAnterior = table.Column<int>(type: "integer", nullable: false),
                    StockPosterior = table.Column<int>(type: "integer", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    SucursalOrigenId = table.Column<int>(type: "integer", nullable: false),
                    SucursalDestinoId = table.Column<int>(type: "integer", nullable: true),
                    CompraId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Sucursales_SucursalDestinoId",
                        column: x => x.SucursalDestinoId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Sucursales_SucursalOrigenId",
                        column: x => x.SucursalOrigenId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdopcion_Estado",
                table: "SolicitudesAdopcion",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Estado",
                table: "Pagos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdentificadorExterno",
                table: "Pagos",
                column: "IdentificadorExterno");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_NumeroPago",
                table: "Pagos",
                column: "NumeroPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ProveedorPago",
                table: "Pagos",
                column: "ProveedorPago");

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_Nombre",
                table: "Mascotas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Cargo",
                table: "Empleados",
                column: "Cargo");

            migrationBuilder.CreateIndex(
                name: "IX_Duenos_Email",
                table: "Duenos",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Duenos_Nombre",
                table: "Duenos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_Estado",
                table: "Consultas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_FechaConsulta",
                table: "Consultas",
                column: "FechaConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Accion",
                table: "AuditLogs",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entidad",
                table: "AuditLogs",
                column: "Entidad");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FechaCreacion",
                table: "AuditLogs",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UsuarioEmail",
                table: "AuditLogs",
                column: "UsuarioEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProveedorId",
                table: "Compras",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_SucursalId",
                table: "Compras",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_CompraId",
                table: "DetallesCompra",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_ProductoId",
                table: "DetallesCompra",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ProductoId_SucursalId",
                table: "Inventarios",
                columns: new[] { "ProductoId", "SucursalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_SucursalId",
                table: "Inventarios",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_CompraId",
                table: "MovimientosInventario",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                table: "MovimientosInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalDestinoId",
                table: "MovimientosInventario",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalOrigenId",
                table: "MovimientosInventario",
                column: "SucursalOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                table: "MovimientosInventario",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoBarras",
                table: "Productos",
                column: "CodigoBarras");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DetallesCompra");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesAdopcion_Estado",
                table: "SolicitudesAdopcion");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_Estado",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdentificadorExterno",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_NumeroPago",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ProveedorPago",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Mascotas_Nombre",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_Cargo",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Duenos_Email",
                table: "Duenos");

            migrationBuilder.DropIndex(
                name: "IX_Duenos_Nombre",
                table: "Duenos");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_Estado",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_FechaConsulta",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "SolicitudesAdopcion");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Duenos");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "EliminadoPor",
                table: "AnimalesAdopcion");
        }
    }
}
