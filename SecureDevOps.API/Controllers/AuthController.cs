using Microsoft.AspNetCore.Mvc;
using SecureDevOps.API.DTOs.Auth;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(Register), new { id = result.User!.Id }, result.User);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result is null)
            return Unauthorized("Invalid credentials");

        return Ok(result);
    }

    // Logout: confirma el cierre de sesión. En un esquema JWT sin estado, la revocación
    // inmediata del token requiere un blocklist/denylist. Aquí se devuelve 204 y el cliente
    // elimina el token localmente.
    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Logout()
    {
        return NoContent();
    }
}
