using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaVencimientoAjusteAndSerieAjuste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "producto_serie",
                type: "integer",
                precision: 32,
                scale: 0,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldPrecision: 32);

            migrationBuilder.AddColumn<int>(
                name: "id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie",
                type: "integer",
                precision: 32,
                scale: 0,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaVencimiento",
                schema: "BodegaDESAM",
                table: "detalle_ajuste",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_serie_id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie",
                column: "id_detalle_ajuste");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_serie_detalle_ajuste_id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie",
                column: "id_detalle_ajuste",
                principalSchema: "BodegaDESAM",
                principalTable: "detalle_ajuste",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producto_serie_detalle_ajuste_id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie");

            migrationBuilder.DropIndex(
                name: "IX_producto_serie_id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie");

            migrationBuilder.DropColumn(
                name: "id_detalle_ajuste",
                schema: "BodegaDESAM",
                table: "producto_serie");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                schema: "BodegaDESAM",
                table: "detalle_ajuste");

            migrationBuilder.AlterColumn<int>(
                name: "id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "producto_serie",
                type: "integer",
                precision: 32,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldPrecision: 32,
                oldScale: 0,
                oldNullable: true);
        }
    }
}
