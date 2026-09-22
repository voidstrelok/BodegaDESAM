using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    public partial class AddValorUnitario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "valor_unitario",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "valor_unitario",
                schema: "BodegaDESAM",
                table: "detalle_ajuste",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valor_unitario",
                schema: "BodegaDESAM",
                table: "detalle_entrada");

            migrationBuilder.DropColumn(
                name: "valor_unitario",
                schema: "BodegaDESAM",
                table: "detalle_ajuste");
        }
    }
}
