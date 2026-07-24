using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Destinatario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Asunto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TipoNotificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Intentos = table.Column<int>(type: "integer", nullable: false),
                    MensajeError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContenidoHtml = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_Estado",
                table: "EmailLogs",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_FechaSolicitud",
                table: "EmailLogs",
                column: "FechaSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_TipoNotificacion",
                table: "EmailLogs",
                column: "TipoNotificacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLogs");
        }
    }
}
