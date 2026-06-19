using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasFelices.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFotoAnimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "AnimalesAdopcion",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "AnimalesAdopcion");
        }
    }
}
