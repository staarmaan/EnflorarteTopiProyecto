using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnflorarteTopiProyecto.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class relacionComandaArreglo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "arreglo",
                columns: table => new
                {
                    arreglo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    foto_ruta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arreglo", x => x.arreglo_id);
                });

            migrationBuilder.CreateTable(
                name: "flor",
                columns: table => new
                {
                    flor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    foto_ruta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flor", x => x.flor_id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contrasena = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "arreglo_flor",
                columns: table => new
                {
                    arreglo_id = table.Column<int>(type: "integer", nullable: false),
                    flor_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arreglo_flor", x => new { x.arreglo_id, x.flor_id });
                    table.ForeignKey(
                        name: "FK_arreglo_flor_arreglo_arreglo_id",
                        column: x => x.arreglo_id,
                        principalTable: "arreglo",
                        principalColumn: "arreglo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_arreglo_flor_flor_flor_id",
                        column: x => x.flor_id,
                        principalTable: "flor",
                        principalColumn: "flor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flor_inventario_color",
                columns: table => new
                {
                    flor_inventario_color_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flor_id = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flor_inventario_color", x => x.flor_inventario_color_id);
                    table.ForeignKey(
                        name: "FK_flor_inventario_color_flor_flor_id",
                        column: x => x.flor_id,
                        principalTable: "flor",
                        principalColumn: "flor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comanda",
                columns: table => new
                {
                    comanda_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    repartidor_id = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    liquidado = table.Column<bool>(type: "boolean", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cliente_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    link_direccion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    domicilio_referencias = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    numero_ruta = table.Column<int>(type: "integer", nullable: true),
                    tipo_entrega = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    direccion_entrega = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_entrega = table.Column<DateTime>(type: "date", nullable: false),
                    hora_entrega = table.Column<TimeSpan>(type: "time", nullable: false),
                    arreglo_id = table.Column<int>(type: "integer", nullable: false),
                    precio_arreglo = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    pago_envio = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    cantidad_arreglo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    mensaje_arreglo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    anticipo_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    anticipo_total = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda", x => x.comanda_id);
                    table.CheckConstraint("CK_Comanda_AnticipoTotal_NonNegative", "anticipo_total >= 0");
                    table.CheckConstraint("CK_Comanda_CantidadArreglo_ValidRange", "cantidad_arreglo BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_Comanda_NumeroRuta_PositiveOrNull", "numero_ruta IS NULL OR numero_ruta > 0");
                    table.CheckConstraint("CK_Comanda_PagoEnvio_NonNegative", "pago_envio >= 0");
                    table.CheckConstraint("CK_Comanda_PrecioArreglo_NonNegative", "precio_arreglo >= 0");
                    table.ForeignKey(
                        name: "FK_comanda_arreglo_arreglo_id",
                        column: x => x.arreglo_id,
                        principalTable: "arreglo",
                        principalColumn: "arreglo_id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_arreglo_flor_flor_id",
                table: "arreglo_flor",
                column: "flor_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_arreglo_id",
                table: "comanda",
                column: "arreglo_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_repartidor_id",
                table: "comanda",
                column: "repartidor_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_usuario_id",
                table: "comanda",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_flor_inventario_color_flor_id_color",
                table: "flor_inventario_color",
                columns: new[] { "flor_id", "color" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arreglo_flor");

            migrationBuilder.DropTable(
                name: "comanda");

            migrationBuilder.DropTable(
                name: "flor_inventario_color");

            migrationBuilder.DropTable(
                name: "arreglo");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "flor");
        }
    }
}
