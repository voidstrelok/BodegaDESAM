using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BodegaDESAM.Controllers;

[AllowAnonymous]
[Route("auth")]
public sealed class AuthController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(
        [FromQuery] string email,
        [FromQuery] string password,
        [FromQuery] bool rememberMe = false,
        [FromQuery] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Redirect("/login");

        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Redirect("/login?error=1");

        // El dashboard es siempre el punto de entrada después de autenticarse.
        return Redirect("/dashboard");
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }
}
