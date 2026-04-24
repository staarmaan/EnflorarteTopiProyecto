using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CamposFaltantesComandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CajaTipoArreglo",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorEvolturaArreglo",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolturaArreglo",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroPedido",
                table: "comanda",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoArreglo",
                table: "comanda",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CajaTipoArreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "ColorEvolturaArreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "EvolturaArreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "NumeroPedido",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "TipoArreglo",
                table: "comanda");
        }
    }
}
