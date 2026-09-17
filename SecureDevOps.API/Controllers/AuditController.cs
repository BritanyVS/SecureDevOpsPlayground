using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;

namespace SecureDevOps.API.Controllers;

// ⚠️ LABORATORIO: los "logs de auditoría" deberían exigir rol Admin y solo mostrar
// la actividad propia. Aquí cualquier usuario autenticado ve actividad y correos.
// FIX: [Authorize(Roles = "Admin")] + registrar entradas reales en una tabla de logs.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var recentTasks = await _db.TaskItems
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .Select(t => new
            {
                t.Title,
                t.UpdatedAt,
                User = t.CreatedByUser.Username,
                Email = t.CreatedByUser.Email,
                t.Status
            })
            .ToListAsync();

        var lines = recentTasks.Select(t => new
        {
            timestamp = t.UpdatedAt.ToString("s"),
            level = "info",
            actor = t.User,
            email = t.Email,
            message = $"Task '{t.Title}' updated to {t.Status}"
        }).ToList();

        return Ok(new { logs = lines, count = lines.Count });
    }
}