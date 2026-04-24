using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class camposFaltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccesorioArreglo",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedioDeLaSolicitud",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreReceptorEnvio",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TelefonoReceptorEnvio",
                table: "comanda",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccesorioArreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "MedioDeLaSolicitud",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "NombreReceptorEnvio",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "TelefonoReceptorEnvio",
                table: "comanda");
        }
    }
}
