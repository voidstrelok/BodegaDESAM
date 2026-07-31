using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BodegaDESAM.Components.Auth;

[Authorize]
public abstract class AuthorizedPage : ComponentBase
{
}
