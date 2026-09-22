using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services;

public sealed class AuditFilter
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public string? UsuarioId { get; set; }
    public string? Accion { get; set; }
    public string? Entidad { get; set; }
    public int? BodegaId { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = PaginationDefaults.DefaultPageSize;
    public string? Texto { get; set; }
}
public sealed record AuditPage(List<AuditLog> Registros, int Total, int Pagina, int TotalPaginas);
public sealed class AuditQueryService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    public AuditQueryService(IDbContextFactory<PostgresDataContext> factory) => _factory = factory;
    public async Task<AuditPage> BuscarAsync(AuditFilter filtro)
    {
        using var db = _factory.CreateDbContext();
        var query = db.AuditLog.AsNoTracking().Include(x => x.Bodega).AsQueryable();
        if (filtro.Desde.HasValue) query = query.Where(x => x.FechaHora >= filtro.Desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filtro.Hasta.HasValue) query = query.Where(x => x.FechaHora < filtro.Hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioId)) query = query.Where(x => x.UsuarioId == filtro.UsuarioId);
        if (!string.IsNullOrWhiteSpace(filtro.Accion)) query = query.Where(x => x.Accion == filtro.Accion);
        if (!string.IsNullOrWhiteSpace(filtro.Entidad)) query = query.Where(x => x.Entidad == filtro.Entidad);
        if (filtro.BodegaId.HasValue) query = query.Where(x => x.BodegaId == filtro.BodegaId);
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.UsuarioId, $"%{texto}%") ||
                EF.Functions.ILike(x.Accion, $"%{texto}%") ||
                EF.Functions.ILike(x.Entidad, $"%{texto}%") ||
                EF.Functions.ILike(x.Detalle ?? string.Empty, $"%{texto}%") ||
                EF.Functions.ILike(x.Bodega!.Nombre, $"%{texto}%") ||
                db.Users.Any(u => u.Id == x.UsuarioId && EF.Functions.ILike(u.Email ?? string.Empty, $"%{texto}%")));
        }

        filtro.TamanoPagina = PaginationDefaults.AllowedPageSizes.Contains(filtro.TamanoPagina)
            ? filtro.TamanoPagina
            : PaginationDefaults.DefaultPageSize;
        var total = await query.CountAsync();
        var pagina = Math.Max(1, filtro.Pagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)filtro.TamanoPagina));
        pagina = Math.Min(pagina, totalPaginas);
        var registros = await query.OrderByDescending(x => x.FechaHora).ThenByDescending(x => x.Id)
            .Skip((pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return new AuditPage(registros, total, pagina, totalPaginas);
    }
}
