using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AddVentaIdToPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroPago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ProveedorPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentificadorExterno = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TokenPasarela = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UrlAprobacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstadoExterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IntentosVerificacion = table.Column<int>(type: "integer", nullable: false),
                    MensajeRespuesta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    ConsultaId = table.Column<int>(type: "integer", nullable: true),
                    DuenoId = table.Column<int>(type: "integer", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EliminadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pagos_Duenos_DuenoId",
                        column: x => x.DuenoId,
                        principalTable: "Duenos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ConsultaId",
                table: "Pagos",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_DuenoId",
                table: "Pagos",
                column: "DuenoId");

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
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");
        }
    }
}
