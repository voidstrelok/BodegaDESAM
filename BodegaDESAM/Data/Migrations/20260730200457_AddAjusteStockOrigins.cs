using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAjusteStockOrigins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida",
                type: "integer",
                precision: 32,
                scale: 0,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida",
                column: "id_detalle_ajuste_origen");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_detalle_ajuste_origen");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_detalle_entrada_origen");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_lote");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_ajuste_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_detalle_ajuste_origen",
                principalSchema: "Bodega_dev",
                principalTable: "detalle_ajuste",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_ajuste_detalle_entrada_id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_detalle_entrada_origen",
                principalSchema: "Bodega_dev",
                principalTable: "detalle_entrada",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_ajuste_lote_id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste",
                column: "id_lote",
                principalSchema: "Bodega_dev",
                principalTable: "lote",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_salida_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida",
                column: "id_detalle_ajuste_origen",
                principalSchema: "Bodega_dev",
                principalTable: "detalle_ajuste",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalle_ajuste_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_ajuste_detalle_entrada_id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_ajuste_lote_id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_salida_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida");

            migrationBuilder.DropIndex(
                name: "IX_detalle_salida_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida");

            migrationBuilder.DropIndex(
                name: "IX_detalle_ajuste_id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropIndex(
                name: "IX_detalle_ajuste_id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropIndex(
                name: "IX_detalle_ajuste_id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropColumn(
                name: "id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_salida");

            migrationBuilder.DropColumn(
                name: "id_detalle_ajuste_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropColumn(
                name: "id_detalle_entrada_origen",
                schema: "Bodega_dev",
                table: "detalle_ajuste");

            migrationBuilder.DropColumn(
                name: "id_lote",
                schema: "Bodega_dev",
                table: "detalle_ajuste");
        }
    }
}
