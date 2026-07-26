using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class SucursalYRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios");

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalDestinoId",
                table: "MovimientosInventario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "MovimientosInventario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Inventarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Empleados",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Consultas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Compras",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Ciudad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EsPrincipal = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_SucursalId",
                table: "Ventas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalDestinoId",
                table: "MovimientosInventario",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalId",
                table: "MovimientosInventario",
                column: "SucursalId");

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
                name: "IX_Empleados_SucursalId",
                table: "Empleados",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_SucursalId",
                table: "Consultas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_SucursalId",
                table: "Compras",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_Ciudad",
                table: "Sucursales",
                column: "Ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_Nombre",
                table: "Sucursales",
                column: "Nombre");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Sucursales_SucursalId",
                table: "Compras",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Sucursales_SucursalId",
                table: "Consultas",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Sucursales_SucursalId",
                table: "Empleados",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.InsertData(
                table: "Sucursales",
                columns: new[] { "Nombre", "Direccion", "Telefono", "Email", "Ciudad", "EsPrincipal", "Activo", "FechaCreacion", "FechaActualizacion" },
                values: new object[] { "Huellitas Felices - Sede Principal", "Av. Amazonas y Naciones Unidas, Quito", "(02) 234-5678", "sede@huellitas.ec", "Quito", true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.Sql("UPDATE \"Inventarios\" SET \"SucursalId\" = 1 WHERE \"SucursalId\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Sucursales_SucursalId",
                table: "Inventarios",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalDestinoId",
                table: "MovimientosInventario",
                column: "SucursalDestinoId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalId",
                table: "MovimientosInventario",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Sucursales_SucursalId",
                table: "Ventas",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Sucursales_SucursalId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Sucursales_SucursalId",
                table: "Consultas");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Sucursales_SucursalId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Sucursales_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalId",
                table: "MovimientosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Sucursales_SucursalId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_SucursalId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_SucursalId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ProductoId_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_SucursalId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_SucursalId",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Compras_SucursalId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Compras");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                unique: true);
        }
    }
}
