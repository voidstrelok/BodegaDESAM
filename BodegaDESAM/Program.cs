using BodegaDESAM;
using BodegaDESAM.Components;
using BodegaDESAM.Data;
using BodegaDESAM.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllersWithViews();

//BDD - Contexto unificado con Identity
builder.Services.AddDbContextFactory<PostgresDataContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresDataContext"),
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "BodegaDESAM")));

// Registrar también como DbContext normal para Identity
builder.Services.AddDbContext<PostgresDataContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresDataContext"),
        o => o.MigrationsHistoryTable("__EFMigrationsHistory", "BodegaDESAM")));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<PostgresDataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, UserClaimsPrincipalFactory<IdentityUser, IdentityRole>>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Al desactivar un usuario, su sesión se invalida en la siguiente solicitud.
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CategoriaProductoService>();
builder.Services.AddScoped<ProductoSerieService>();
builder.Services.AddScoped<MarcaService>();
builder.Services.AddScoped<ModeloService>();
builder.Services.AddScoped<EntradaService>();
builder.Services.AddScoped<LoteService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<BodegaService>();
builder.Services.AddScoped<BodegaContextService>();
builder.Services.AddScoped<UsuarioBodegaService>();
builder.Services.AddScoped<BodegaAuthorizationService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<InventarioDetalleService>();
builder.Services.AddScoped<SalidaService>();
builder.Services.AddScoped<EstablecimientoService>();
builder.Services.AddScoped<SalidaPdfReportService>();
builder.Services.AddScoped<EntradaPdfReportService>();
builder.Services.AddScoped<MovimientoProductoService>();
builder.Services.AddScoped<UserDisplayNameService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AuditContextService>();
builder.Services.AddScoped<AuditQueryService>();
builder.Services.AddScoped<AlertaService>();
builder.Services.AddScoped<AlertaStockService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AjusteInventarioService>();
builder.Services.AddScoped<LoadingState>();
builder.Services.AddSingleton<ThemeService>(); // Singleton para mantener estado entre navegaciones
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PostgresDataContext>();

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("BodegaDESAM");
}

var app = builder.Build();

// El esquema debe estar vigente antes de inicializar Identity o atender solicitudes.
// MigrateAsync es idempotente: sólo aplica las migraciones aún pendientes.
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
    logger.LogInformation("Verificando migraciones pendientes de la base de datos.");
    var db = scope.ServiceProvider.GetRequiredService<PostgresDataContext>();
    await db.Database.MigrateAsync();
    logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");
}

await IdentitySeed.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await AppSeed.SeedAsync(app.Services);
}

//

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue<bool>("ReverseProxy:Enabled"))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
