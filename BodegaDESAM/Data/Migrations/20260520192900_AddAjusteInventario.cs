using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAjusteInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ajuste_inventario",
                schema: "Bodega_dev",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUsuario = table.Column<string>(type: "text", nullable: false),
                    id_bodega = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ajuste_inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ajuste_inventario_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "Bodega_dev",
                        principalTable: "bodega",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "detalle_ajuste",
                schema: "Bodega_dev",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_ajuste = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_marca = table.Column<long>(type: "bigint", nullable: false),
                    id_modelo = table.Column<long>(type: "bigint", nullable: true),
                    TipoAjuste = table.Column<string>(type: "text", nullable: false),
                    Cantidad = table.Column<long>(type: "bigint", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_ajuste", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_ajuste_inventario_id_ajuste",
                        column: x => x.id_ajuste,
                        principalSchema: "Bodega_dev",
                        principalTable: "ajuste_inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_marca_id_marca",
                        column: x => x.id_marca,
                        principalSchema: "Bodega_dev",
                        principalTable: "marca",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_modelo_id_modelo",
                        column: x => x.id_modelo,
                        principalSchema: "Bodega_dev",
                        principalTable: "modelo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "Bodega_dev",
                        principalTable: "producto",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ajuste_inventario_id_bodega",
                schema: "Bodega_dev",
                table: "ajuste_inventario",
                column: "id_bodega");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_ajuste",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_ajuste");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_marca",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_modelo",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_modelo");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_producto",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalle_ajuste",
                schema: "Bodega_dev");

            migrationBuilder.DropTable(
                name: "ajuste_inventario",
                schema: "Bodega_dev");
        }
    }
}
