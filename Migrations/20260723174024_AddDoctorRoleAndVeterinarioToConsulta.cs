using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorRoleAndVeterinarioToConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Empleados",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VeterinarioId",
                table: "Consultas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Email",
                table: "Empleados",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_VeterinarioId",
                table: "Consultas",
                column: "VeterinarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Empleados_VeterinarioId",
                table: "Consultas",
                column: "VeterinarioId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Empleados_VeterinarioId",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_Email",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_VeterinarioId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "VeterinarioId",
                table: "Consultas");
        }
    }
}
