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

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CategoriaProductoService>();
builder.Services.AddScoped<ProductoSerieService>();
builder.Services.AddScoped<MarcaService>();
builder.Services.AddScoped<ModeloService>();
builder.Services.AddScoped<EntradaService>();
builder.Services.AddScoped<LoteService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<BodegaService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<InventarioDetalleService>();
builder.Services.AddScoped<SalidaService>();
builder.Services.AddScoped<EstablecimientoService>();
builder.Services.AddScoped<SalidaPdfReportService>();
builder.Services.AddScoped<EntradaPdfReportService>();
builder.Services.AddScoped<MovimientoProductoService>();
builder.Services.AddScoped<UserDisplayNameService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AlertaService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AjusteInventarioService>();
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

// Aplicar migraciones pendientes automáticamente (necesario en entornos contenerizados)
if (builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PostgresDataContext>();
    await db.Database.MigrateAsync();
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
