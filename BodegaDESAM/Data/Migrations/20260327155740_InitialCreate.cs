using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BodegaDESAM.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BodegaDESAM");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bodega",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "character varying", maxLength: 200, nullable: true),
                    es_principal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bodega", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria_producto",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_producto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "establecimiento",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_establecimiento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marca",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proveedor",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rut = table.Column<string>(type: "character varying", maxLength: 20, nullable: true),
                    nombre = table.Column<string>(type: "character varying", maxLength: 200, nullable: false),
                    direccion = table.Column<string>(type: "character varying", maxLength: 200, nullable: true),
                    telefono = table.Column<string>(type: "character varying", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying", maxLength: 100, nullable: true),
                    persona_contacto = table.Column<string>(type: "character varying", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "BodegaDESAM",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ubicacion",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_bodega = table.Column<int>(type: "integer", nullable: false),
                    pasillo = table.Column<string>(type: "character varying", maxLength: 10, nullable: false),
                    estante = table.Column<string>(type: "character varying", maxLength: 10, nullable: false),
                    nivel = table.Column<string>(type: "character varying", maxLength: 10, nullable: true),
                    posicion = table.Column<string>(type: "character varying", maxLength: 10, nullable: true),
                    codigo_completo = table.Column<string>(type: "character varying", maxLength: 50, nullable: false),
                    capacidad_maxima = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bloqueada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    motivo_bloqueo = table.Column<string>(type: "character varying", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_ubicacion_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "BodegaDESAM",
                        principalTable: "bodega",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "producto",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_categoria_producto = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false),
                    nombre = table.Column<string>(type: "character varying", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.id);
                    table.ForeignKey(
                        name: "FK_producto_categoria_producto_id_categoria_producto",
                        column: x => x.id_categoria_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "categoria_producto",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "salida",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_bodega = table.Column<int>(type: "integer", nullable: false),
                    id_establecimiento = table.Column<int>(type: "integer", nullable: false),
                    observacion = table.Column<string>(type: "character varying", maxLength: 200, nullable: false),
                    solicitante = table.Column<string>(type: "character varying", maxLength: 100, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salida", x => x.id);
                    table.ForeignKey(
                        name: "FK_salida_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "BodegaDESAM",
                        principalTable: "bodega",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_salida_establecimiento_id_establecimiento",
                        column: x => x.id_establecimiento,
                        principalSchema: "BodegaDESAM",
                        principalTable: "establecimiento",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "modelo",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_marca = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false),
                    nombre = table.Column<string>(type: "character varying", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modelo", x => x.id);
                    table.ForeignKey(
                        name: "FK_modelo_marca_id_marca",
                        column: x => x.id_marca,
                        principalSchema: "BodegaDESAM",
                        principalTable: "marca",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "entrada",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_bodega = table.Column<int>(type: "integer", nullable: false),
                    id_proveedor = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "date", nullable: false),
                    n_documento = table.Column<string>(type: "character varying", maxLength: 50, nullable: false),
                    observacion = table.Column<string>(type: "character varying", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entrada", x => x.id);
                    table.ForeignKey(
                        name: "FK_entrada_bodega_id_bodega",
                        column: x => x.id_bodega,
                        principalSchema: "BodegaDESAM",
                        principalTable: "bodega",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_entrada_proveedor_id_proveedor",
                        column: x => x.id_proveedor,
                        principalSchema: "BodegaDESAM",
                        principalTable: "proveedor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lote",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    codigo = table.Column<string>(type: "character varying", maxLength: 50, nullable: false),
                    fecha_fabricacion = table.Column<DateTime>(type: "date", nullable: true),
                    fecha_vencimiento = table.Column<DateTime>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "character varying", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lote", x => x.id);
                    table.ForeignKey(
                        name: "FK_lote_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "producto",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "detalle_entrada",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_entrada = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_marca = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false),
                    id_modelo = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    id_lote = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    id_ubicacion = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    cantidad = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_entrada", x => x.id);
                    table.ForeignKey(
                        name: "FK_detalle_entrada_entrada_id_entrada",
                        column: x => x.id_entrada,
                        principalSchema: "BodegaDESAM",
                        principalTable: "entrada",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_entrada_lote_id_lote",
                        column: x => x.id_lote,
                        principalSchema: "BodegaDESAM",
                        principalTable: "lote",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_entrada_marca_id_marca",
                        column: x => x.id_marca,
                        principalSchema: "BodegaDESAM",
                        principalTable: "marca",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_entrada_modelo_id_modelo",
                        column: x => x.id_modelo,
                        principalSchema: "BodegaDESAM",
                        principalTable: "modelo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_entrada_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "producto",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_entrada_ubicacion_id_ubicacion",
                        column: x => x.id_ubicacion,
                        principalSchema: "BodegaDESAM",
                        principalTable: "ubicacion",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "detalle_salida",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_salida = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_producto = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_marca = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: false),
                    id_modelo = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    id_lote = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    id_ubicacion = table.Column<long>(type: "bigint", precision: 32, scale: 0, nullable: true),
                    cantidad = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_salida", x => x.id);
                    table.ForeignKey(
                        name: "FK_detalle_salida_lote_id_lote",
                        column: x => x.id_lote,
                        principalSchema: "BodegaDESAM",
                        principalTable: "lote",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_salida_marca_id_marca",
                        column: x => x.id_marca,
                        principalSchema: "BodegaDESAM",
                        principalTable: "marca",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_salida_modelo_id_modelo",
                        column: x => x.id_modelo,
                        principalSchema: "BodegaDESAM",
                        principalTable: "modelo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_salida_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "producto",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_salida_salida_id_salida",
                        column: x => x.id_salida,
                        principalSchema: "BodegaDESAM",
                        principalTable: "salida",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_detalle_salida_ubicacion_id_ubicacion",
                        column: x => x.id_ubicacion,
                        principalSchema: "BodegaDESAM",
                        principalTable: "ubicacion",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "producto_serie",
                schema: "BodegaDESAM",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_detalle_entrada = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: false),
                    id_salida = table.Column<int>(type: "integer", precision: 32, scale: 0, nullable: true),
                    serie = table.Column<string>(type: "character varying", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_serie", x => x.id);
                    table.ForeignKey(
                        name: "FK_producto_serie_detalle_entrada_id_detalle_entrada",
                        column: x => x.id_detalle_entrada,
                        principalSchema: "BodegaDESAM",
                        principalTable: "detalle_entrada",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_producto_serie_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "BodegaDESAM",
                        principalTable: "producto",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_producto_serie_salida_id_salida",
                        column: x => x.id_salida,
                        principalSchema: "BodegaDESAM",
                        principalTable: "salida",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "BodegaDESAM",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "BodegaDESAM",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "BodegaDESAM",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "BodegaDESAM",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "BodegaDESAM",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "BodegaDESAM",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "BodegaDESAM",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bodega_codigo",
                schema: "BodegaDESAM",
                table: "bodega",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_entrada",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_entrada");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_lote",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_lote");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_marca",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_modelo",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_modelo");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_producto",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_entrada_id_ubicacion",
                schema: "BodegaDESAM",
                table: "detalle_entrada",
                column: "id_ubicacion");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_lote",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_lote");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_marca",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_modelo",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_modelo");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_producto",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_salida",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_salida");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_salida_id_ubicacion",
                schema: "BodegaDESAM",
                table: "detalle_salida",
                column: "id_ubicacion");

            migrationBuilder.CreateIndex(
                name: "IX_entrada_id_bodega",
                schema: "BodegaDESAM",
                table: "entrada",
                column: "id_bodega");

            migrationBuilder.CreateIndex(
                name: "IX_entrada_id_proveedor",
                schema: "BodegaDESAM",
                table: "entrada",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_lote_id_producto_codigo",
                schema: "BodegaDESAM",
                table: "lote",
                columns: new[] { "id_producto", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modelo_id_marca_nombre",
                schema: "BodegaDESAM",
                table: "modelo",
                columns: new[] { "id_marca", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_id_categoria_producto",
                schema: "BodegaDESAM",
                table: "producto",
                column: "id_categoria_producto");

            migrationBuilder.CreateIndex(
                name: "IX_producto_serie_id_detalle_entrada",
                schema: "BodegaDESAM",
                table: "producto_serie",
                column: "id_detalle_entrada");

            migrationBuilder.CreateIndex(
                name: "IX_producto_serie_id_producto_serie",
                schema: "BodegaDESAM",
                table: "producto_serie",
                columns: new[] { "id_producto", "serie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_serie_id_salida",
                schema: "BodegaDESAM",
                table: "producto_serie",
                column: "id_salida");

            migrationBuilder.CreateIndex(
                name: "IX_salida_id_bodega",
                schema: "BodegaDESAM",
                table: "salida",
                column: "id_bodega");

            migrationBuilder.CreateIndex(
                name: "IX_salida_id_establecimiento",
                schema: "BodegaDESAM",
                table: "salida",
                column: "id_establecimiento");

            migrationBuilder.CreateIndex(
                name: "IX_ubicacion_id_bodega_codigo_completo",
                schema: "BodegaDESAM",
                table: "ubicacion",
                columns: new[] { "id_bodega", "codigo_completo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "detalle_salida",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "producto_serie",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "detalle_entrada",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "salida",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "entrada",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "lote",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "modelo",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "ubicacion",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "establecimiento",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "proveedor",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "producto",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "marca",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "bodega",
                schema: "BodegaDESAM");

            migrationBuilder.DropTable(
                name: "categoria_producto",
                schema: "BodegaDESAM");
        }
    }
}
