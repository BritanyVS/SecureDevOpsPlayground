using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;
using SecureDevOps.API.DTOs;
using SecureDevOps.API.DTOs.TaskItem;

namespace SecureDevOps.API.Controllers;

// Dashboard / analytics. Incluye un endpoint con ORDER BY inyectable
// (SQL Injection lab) y conteos por estado/prioridad.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(
        [FromQuery] string? recentOrder = "CreatedAt desc")
    {
        var total = await _db.TaskItems.CountAsync();

        var stats = new DashboardStatsDto
        {
            TotalTasks = total,
            Pending = await _db.TaskItems.CountAsync(t => t.Status == Models.Enums.TaskItemStatus.Pending),
            InProgress = await _db.TaskItems.CountAsync(t => t.Status == Models.Enums.TaskItemStatus.InProgress),
            Completed = await _db.TaskItems.CountAsync(t => t.Status == Models.Enums.TaskItemStatus.Completed),
            LowPriority = await _db.TaskItems.CountAsync(t => t.Priority == Models.Enums.TaskPriority.Low),
            MediumPriority = await _db.TaskItems.CountAsync(t => t.Priority == Models.Enums.TaskPriority.Medium),
            HighPriority = await _db.TaskItems.CountAsync(t => t.Priority == Models.Enums.TaskPriority.High)
        };

        // [5] VULNERABLE: SQL Injection en ORDER BY (`recentOrder` se concatena sin validar).
        // GET /api/dashboard/stats?recentOrder=Id;SELECT%20...--   (ORDER BY injection)
        // FIX: validar contra una lista blanca (CreatedAt, Priority, Title) y dirección.
        var recent = await _db.TaskItems
            .FromSqlRaw($"SELECT * FROM TaskItems ORDER BY {recentOrder} LIMIT 5")
            .Include(t => t.AssignedToUser)
            .ToListAsync();

        stats.RecentTasks = recent.Select(t => new TaskItemResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            CreatedByUserId = t.CreatedByUserId,
            AssignedToUserName = t.AssignedToUser?.Username,
            DueDate = t.DueDate,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();

        stats.ByStatus = new Dictionary<string, int>
        {
            ["Pending"] = stats.Pending,
            ["InProgress"] = stats.InProgress,
            ["Completed"] = stats.Completed
        };
        stats.ByPriority = new Dictionary<string, int>
        {
            ["Low"] = stats.LowPriority,
            ["Medium"] = stats.MediumPriority,
            ["High"] = stats.HighPriority
        };

        stats.TopAssignees = await _db.TaskItems
            .Where(t => t.AssignedToUser != null)
            .GroupBy(t => t.AssignedToUser!.Username)
            .Select(g => new TopAssigneeDto { Username = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        return Ok(stats);
    }
}