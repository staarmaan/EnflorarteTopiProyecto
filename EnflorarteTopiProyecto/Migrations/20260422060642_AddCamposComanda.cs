using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var esPostgreSql = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            migrationBuilder.AddColumn<int>(
                name: "cantidad_arreglo",
                table: "comanda",
                nullable: false,
                defaultValue: 1);

            if (esPostgreSql)
            {
                migrationBuilder.AddColumn<string>(
                    name: "domicilio_referencias",
                    table: "comanda",
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "link_direccion",
                    table: "comanda",
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "mensaje_arreglo",
                    table: "comanda",
                    type: "character varying(500)",
                    maxLength: 500,
                    nullable: true);
            }
            else
            {
                migrationBuilder.AddColumn<string>(
                    name: "domicilio_referencias",
                    table: "comanda",
                    type: "nvarchar(500)",
                    maxLength: 500,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "link_direccion",
                    table: "comanda",
                    type: "nvarchar(1000)",
                    maxLength: 1000,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "mensaje_arreglo",
                    table: "comanda",
                    type: "nvarchar(500)",
                    maxLength: 500,
                    nullable: true);
            }

            migrationBuilder.AddColumn<int>(
                name: "numero_ruta",
                table: "comanda",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comanda_CantidadArreglo_ValidRange",
                table: "comanda",
                sql: "cantidad_arreglo BETWEEN 1 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comanda_NumeroRuta_PositiveOrNull",
                table: "comanda",
                sql: "numero_ruta IS NULL OR numero_ruta > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Comanda_CantidadArreglo_ValidRange",
                table: "comanda");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comanda_NumeroRuta_PositiveOrNull",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "cantidad_arreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "domicilio_referencias",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "link_direccion",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "mensaje_arreglo",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "numero_ruta",
                table: "comanda");
        }
    }
}
