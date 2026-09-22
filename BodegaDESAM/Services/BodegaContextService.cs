using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BodegaDESAM.Services;

/// <summary>Contexto de bodega para el circuito Blazor actual. Nunca otorga acceso por sí solo.</summary>
public sealed class BodegaContextService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    private readonly AuthenticationStateProvider _auth;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _idBodegaActiva;

    public event Func<Task>? BodegaCambiada;
    public int? IdBodegaActiva => _idBodegaActiva;

    public BodegaContextService(IDbContextFactory<PostgresDataContext> factory, AuthenticationStateProvider auth, IHttpContextAccessor httpContextAccessor)
    {
        _factory = factory;
        _auth = auth;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<Bodega>> GetBodegasPermitidasAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true) return [];
        using var db = _factory.CreateDbContext();
        IQueryable<Bodega> query = db.Bodega.AsNoTracking().Where(b => b.Activa);
        if (!user.IsInRole(Auth.AppRoles.Admin))
        {
            var idUsuario = user.FindFirstValue(ClaimTypes.NameIdentifier);
            query = query.Where(b => db.UsuarioBodega.Any(x => x.IdUsuario == idUsuario && x.IdBodega == b.Id));
        }
        return await query.OrderByDescending(b => b.EsPrincipal).ThenBy(b => b.Nombre).ToListAsync();
    }

    public async Task<int?> GetBodegaActivaAsync()
    {
        var permitidas = await GetBodegasPermitidasAsync();
        if (_idBodegaActiva.HasValue && permitidas.Any(b => b.Id == _idBodegaActiva.Value)) return _idBodegaActiva;
        var idGuardado = _httpContextAccessor.HttpContext?.Request.Cookies["bodega-activa"];
        if (int.TryParse(idGuardado, out var idBodega) && permitidas.Any(b => b.Id == idBodega))
        {
            _idBodegaActiva = idBodega;
            return _idBodegaActiva;
        }
        _idBodegaActiva = permitidas.FirstOrDefault(b => b.EsPrincipal)?.Id ?? permitidas.FirstOrDefault()?.Id;
        return _idBodegaActiva;
    }

    public async Task<bool> SeleccionarAsync(int idBodega)
    {
        if (!(await GetBodegasPermitidasAsync()).Any(b => b.Id == idBodega)) return false;
        _idBodegaActiva = idBodega;
        if (BodegaCambiada is not null)
            foreach (var handler in BodegaCambiada.GetInvocationList().Cast<Func<Task>>()) await handler();
        return true;
    }

    public async Task<bool> PuedeAccederAsync(int idBodega) =>
        (await GetBodegasPermitidasAsync()).Any(b => b.Id == idBodega);
}
