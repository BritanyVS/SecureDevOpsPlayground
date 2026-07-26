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
}
