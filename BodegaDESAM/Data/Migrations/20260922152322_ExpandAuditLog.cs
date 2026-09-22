using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Entidad_Accion",
                schema: "BodegaDESAM",
                table: "AuditLog",
                columns: new[] { "Entidad", "Accion" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_FechaHora",
                schema: "BodegaDESAM",
                table: "AuditLog",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UsuarioId",
                schema: "BodegaDESAM",
                table: "AuditLog",
                column: "UsuarioId");

            // Completar el ámbito de bodega de las auditorías históricas de movimientos.
            migrationBuilder.Sql(@"
UPDATE ""BodegaDESAM"".""AuditLog"" a SET ""BodegaId"" = e.id_bodega FROM ""BodegaDESAM"".entrada e
WHERE a.""Entidad"" = 'Entrada' AND a.""EntidadId"" = e.id AND a.""BodegaId"" IS NULL;
UPDATE ""BodegaDESAM"".""AuditLog"" a SET ""BodegaId"" = s.id_bodega FROM ""BodegaDESAM"".salida s
WHERE a.""Entidad"" = 'Salida' AND a.""EntidadId"" = s.id AND a.""BodegaId"" IS NULL;
UPDATE ""BodegaDESAM"".""AuditLog"" a SET ""BodegaId"" = j.id_bodega FROM ""BodegaDESAM"".ajuste_inventario j
WHERE a.""Entidad"" = 'AjusteInventario' AND a.""EntidadId"" = j.""Id"" AND a.""BodegaId"" IS NULL;");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_bodega_BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog",
                column: "BodegaId",
                principalSchema: "BodegaDESAM",
                principalTable: "bodega",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_bodega_BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Entidad_Accion",
                schema: "BodegaDESAM",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_FechaHora",
                schema: "BodegaDESAM",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UsuarioId",
                schema: "BodegaDESAM",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                schema: "BodegaDESAM",
                table: "AuditLog");
        }
    }
}
