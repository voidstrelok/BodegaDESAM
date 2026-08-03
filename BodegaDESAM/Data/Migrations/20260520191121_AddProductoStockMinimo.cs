using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoStockMinimo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                schema: "BodegaDESAM",
                table: "producto",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockMinimo",
                schema: "BodegaDESAM",
                table: "producto");
        }
    }
}
