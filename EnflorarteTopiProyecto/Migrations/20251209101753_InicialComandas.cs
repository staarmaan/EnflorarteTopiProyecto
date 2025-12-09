using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations
{
    /// <inheritdoc />
    public partial class InicialComandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usuario",
                newName: "usuario_id");

            migrationBuilder.CreateTable(
                name: "comanda",
                columns: table => new
                {
                    comanda_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    repartidor_id = table.Column<int>(type: "int", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    liquidado = table.Column<bool>(type: "bit", nullable: false),
                    cliente_nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cliente_telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    direccion_entrega = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_entrega = table.Column<DateTime>(type: "date", nullable: false),
                    hora_entrega = table.Column<TimeSpan>(type: "time", nullable: true),
                    tipo_entrega = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    nombre_arreglo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    precio_arreglo = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    pago_envio = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    foto_arreglo_ruta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    anticipo_tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    anticipo_total = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda", x => x.comanda_id);
                    table.CheckConstraint("CK_Comanda_AnticipoTotal_NonNegative", "[anticipo_total] >= 0");
                    table.CheckConstraint("CK_Comanda_PagoEnvio_NonNegative", "[pago_envio] >= 0");
                    table.CheckConstraint("CK_Comanda_PrecioArreglo_NonNegative", "[precio_arreglo] >= 0");
                    table.ForeignKey(
                        name: "FK_comanda_usuario_repartidor_id",
                        column: x => x.repartidor_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_comanda_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_comanda_repartidor_id",
                table: "comanda",
                column: "repartidor_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_usuario_id",
                table: "comanda",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comanda");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "usuario",
                newName: "Id");
        }
    }
}
