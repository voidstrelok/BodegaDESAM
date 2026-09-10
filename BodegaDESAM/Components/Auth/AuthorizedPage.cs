using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using BodegaDESAM.Services;

namespace BodegaDESAM.Components.Auth;

[Authorize]
public abstract class AuthorizedPage : ComponentBase
{
    [Inject]
    protected LoadingState LoadingState { get; set; } = default!;
}
