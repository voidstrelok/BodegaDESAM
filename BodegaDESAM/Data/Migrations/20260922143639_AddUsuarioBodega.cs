using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario_bodega",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_bodega = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_bodega", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_bodega_AspNetUsers_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_bodega_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "BodegaDESAM",
                        principalTable: "bodega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_bodega_id_bodega",
                schema: "BodegaDESAM",
                table: "usuario_bodega",
                column: "id_bodega");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_bodega_id_usuario_id_bodega",
                schema: "BodegaDESAM",
                table: "usuario_bodega",
                columns: new[] { "id_usuario", "id_bodega" },
                unique: true);

            // Los administradores tienen acceso implícito a todas las bodegas. El resto
            // conserva acceso a la bodega principal al activar esta funcionalidad.
            migrationBuilder.Sql(@"
INSERT INTO ""BodegaDESAM"".usuario_bodega (id_usuario, id_bodega)
SELECT u.""Id"", b.id
FROM ""BodegaDESAM"".""AspNetUsers"" u
CROSS JOIN LATERAL (
    SELECT id FROM ""BodegaDESAM"".bodega WHERE activa AND es_principal ORDER BY id LIMIT 1
) b
WHERE NOT EXISTS (
    SELECT 1 FROM ""BodegaDESAM"".""AspNetUserRoles"" ur
    JOIN ""BodegaDESAM"".""AspNetRoles"" r ON r.""Id"" = ur.""RoleId""
    WHERE ur.""UserId"" = u.""Id"" AND r.""Name"" = 'Admin'
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_bodega",
                schema: "BodegaDESAM");
        }
    }
}
