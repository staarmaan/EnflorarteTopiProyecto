using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class boolDeArchivamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archivado",
                table: "comanda",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archivado",
                table: "comanda");
        }
    }
}
