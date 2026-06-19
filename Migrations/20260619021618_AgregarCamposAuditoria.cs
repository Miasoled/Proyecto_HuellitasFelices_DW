using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usamos SQL directo con IF NOT EXISTS para evitar errores si alguna columna ya existe

            migrationBuilder.Sql(@"ALTER TABLE ""SolicitudesAdopcion"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""SolicitudesAdopcion"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""Mascotas"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""Mascotas"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""Empleados"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""Empleados"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""Duenos"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""Duenos"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""Consultas"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""Consultas"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");

            migrationBuilder.Sql(@"ALTER TABLE ""AnimalesAdopcion"" ADD COLUMN IF NOT EXISTS ""FechaActualizacion"" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';");
            migrationBuilder.Sql(@"ALTER TABLE ""AnimalesAdopcion"" ADD COLUMN IF NOT EXISTS ""FechaEliminacion"" timestamp with time zone NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "SolicitudesAdopcion");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "SolicitudesAdopcion");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Duenos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Duenos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "AnimalesAdopcion");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "AnimalesAdopcion");
        }
    }
}
