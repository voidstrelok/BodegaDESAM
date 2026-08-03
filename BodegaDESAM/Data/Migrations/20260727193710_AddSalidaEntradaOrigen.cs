using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalidaEntradaOrigen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                type: "integer",
                precision: 32,
                scale: 0,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_detalle_entrada");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_salida_detalle_entrada_id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_detalle_entrada",
                principalSchema: "BodegaDESAM",
                principalTable: "detalle_entrada",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalle_salida_detalle_entrada_id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida");

            migrationBuilder.DropIndex(
                name: "IX_detalle_salida_id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida");

            migrationBuilder.DropColumn(
                name: "id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "detalle_salida");
        }
    }
}
