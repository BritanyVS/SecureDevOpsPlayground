using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureDevOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var username = User.FindFirst(ClaimTypes.Name)!.Value;
        var email = User.FindFirst(ClaimTypes.Email)!.Value;
        var role = User.FindFirst(ClaimTypes.Role)!.Value;

        return Ok(new
        {
            userId,
            username,
            email,
            role
        });
    }
}
