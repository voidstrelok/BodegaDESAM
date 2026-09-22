using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BodegaDESAM.Services;

public sealed class AuditContextService
{
    private readonly AuthenticationStateProvider _authentication;
    private readonly IHttpContextAccessor _http;
    public AuditContextService(AuthenticationStateProvider authentication, IHttpContextAccessor http) { _authentication = authentication; _http = http; }
    public async Task<(string UsuarioId, string? Ip)> GetActorAsync()
    {
        var user = (await _authentication.GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException("No hay un usuario autenticado para registrar la auditoría.");

        var usuarioId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new InvalidOperationException("El usuario autenticado no tiene un identificador válido para registrar la auditoría.");

        return (usuarioId, _http.HttpContext?.Connection.RemoteIpAddress?.ToString());
    }
}
