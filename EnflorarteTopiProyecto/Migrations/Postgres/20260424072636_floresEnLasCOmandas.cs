using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class floresEnLasCOmandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoArregloRuta",
                table: "comanda",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArreglo",
                table: "comanda",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "comanda_flor",
                columns: table => new
                {
                    comanda_id = table.Column<int>(type: "integer", nullable: false),
                    flor_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    color_seleccionado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda_flor", x => new { x.comanda_id, x.flor_id });
                    table.ForeignKey(
                        name: "FK_comanda_flor_comanda_comanda_id",
                        column: x => x.comanda_id,
                        principalTable: "comanda",
                        principalColumn: "comanda_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comanda_flor_flor_flor_id",
                        column: x => x.flor_id,
                        principalTable: "flor",
                        principalColumn: "flor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comanda_flor_flor_id",
                table: "comanda_flor",
                column: "flor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comanda_flor");

            migrationBuilder.DropColumn(
                name: "FotoArregloRuta",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "NombreArreglo",
                table: "comanda");
        }
    }
}
