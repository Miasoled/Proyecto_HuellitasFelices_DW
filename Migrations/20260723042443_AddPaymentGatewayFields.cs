using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGatewayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoExterno",
                table: "Pagos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacion",
                table: "Pagos",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificadorExterno",
                table: "Pagos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosVerificacion",
                table: "Pagos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MensajeRespuesta",
                table: "Pagos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Pagos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProveedorPago",
                table: "Pagos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenPasarela",
                table: "Pagos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlAprobacion",
                table: "Pagos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoExterno",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IdentificadorExterno",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IntentosVerificacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MensajeRespuesta",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ProveedorPago",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TokenPasarela",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "UrlAprobacion",
                table: "Pagos");
        }
    }
}
