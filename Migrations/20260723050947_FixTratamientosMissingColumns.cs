using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class FixTratamientosMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The DB was created from an older model snapshot that was missing several
            // columns.  Use IF NOT EXISTS so this is safe to re-run.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tratamientos"" ADD COLUMN IF NOT EXISTS ""Dosis"" character varying(50);
                ALTER TABLE ""Tratamientos"" ADD COLUMN IF NOT EXISTS ""Frecuencia"" character varying(100);
                ALTER TABLE ""Tratamientos"" ADD COLUMN IF NOT EXISTS ""DuracionDias"" integer;
                ALTER TABLE ""Tratamientos"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""Consultas"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""Mascotas"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""Duenos"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""Empleados"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""AnimalesAdopcion"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""SolicitudesAdopcion"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);

                ALTER TABLE ""Pagos"" ADD COLUMN IF NOT EXISTS ""NumeroPago"" character varying(20) NOT NULL DEFAULT '';
                ALTER TABLE ""Pagos"" ADD COLUMN IF NOT EXISTS ""EliminadoPor"" character varying(100);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
