using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSucursalAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Sucursales_SucursalId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Sucursales_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalOrigenId",
                table: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_SucursalOrigenId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ProductoId_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_SucursalId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Compras_SucursalId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "SucursalDestinoId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "SucursalOrigenId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Compras");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios");

            migrationBuilder.AddColumn<int>(
                name: "SucursalDestinoId",
                table: "MovimientosInventario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalOrigenId",
                table: "MovimientosInventario",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Inventarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Compras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalDestinoId",
                table: "MovimientosInventario",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalOrigenId",
                table: "MovimientosInventario",
                column: "SucursalOrigenId");

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
                name: "IX_Compras_SucursalId",
                table: "Compras",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Sucursales_SucursalId",
                table: "Compras",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Sucursales_SucursalId",
                table: "Inventarios",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalDestinoId",
                table: "MovimientosInventario",
                column: "SucursalDestinoId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Sucursales_SucursalOrigenId",
                table: "MovimientosInventario",
                column: "SucursalOrigenId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
