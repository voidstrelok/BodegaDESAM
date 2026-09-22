using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertaStockPorBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockMinimo",
                schema: "BodegaDESAM",
                table: "producto");

            migrationBuilder.CreateTable(
                name: "alerta_stock",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_bodega = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    stock_minimo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerta_stock", x => x.id);
                    table.ForeignKey(
                        name: "FK_alerta_stock_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "BodegaDESAM",
                        principalTable: "bodega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_alerta_stock_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerta_stock_id_bodega_id_producto",
                schema: "BodegaDESAM",
                table: "alerta_stock",
                columns: new[] { "id_bodega", "id_producto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alerta_stock_id_producto",
                schema: "BodegaDESAM",
                table: "alerta_stock",
                column: "id_producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerta_stock",
                schema: "BodegaDESAM");

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                schema: "BodegaDESAM",
                table: "producto",
                type: "integer",
                nullable: true);
        }
    }
}
