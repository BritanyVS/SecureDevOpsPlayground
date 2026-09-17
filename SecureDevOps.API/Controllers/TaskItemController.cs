using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDevOps.API.DTOs.TaskItem;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Controllers;

// ⚠️ LABORATORIO: el propietario viene del DTO (CreatedByUserId), NO del token JWT.
// Esto es intencional para que Snyk Code detecte el fallo "Broken Object Level
// Authorization (IDOR)". Ver docs/VULNERABILITIES.md para el fix.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskItemController : ControllerBase
{
    private readonly ITaskItemService _service;

    public TaskItemController(ITaskItemService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemResponseDto>>> GetAll()
    {
        var tasks = await _service.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TaskItemResponseDto>>> Search([FromQuery] string q)
    {
        var tasks = await _service.SearchAsync(q);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItemResponseDto>> GetById(Guid id)
    {
        var task = await _service.GetByIdAsync(id);
        if (task is null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponseDto>> Create(TaskItemCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskItemResponseDto>> Update(Guid id, TaskItemUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskItemResponseDto>> UpdateStatus(Guid id, TaskStatusUpdateDto dto)
    {
        var updated = await _service.UpdateStatusAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // Comentarios de una tarea (stored XSS lab: el Content puede contener script).
    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<IEnumerable<TaskCommentResponseDto>>> GetComments(Guid id)
    {
        var comments = await _service.ListCommentsAsync(id);
        return Ok(comments);
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<TaskCommentResponseDto>> CreateComment(Guid id, TaskCommentCreateDto dto)
    {
        var created = await _service.CreateCommentAsync(id, dto);
        if (created is null) return NotFound();
        return CreatedAtAction(nameof(GetComments), new { id }, created);
    }

    // Exportación CSV de todas las tareas (data exposure lab).
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? format = "csv")
    {
        var tasks = (await _service.ExportTasksAsync()).ToList();
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Solo se soporta format=csv" });

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("id,title,status,priority,assignedTo,dueDate,createdAt,updatedAt");
        foreach (var t in tasks)
        {
            sb.AppendLine(string.Join(",",
                t.Id,
                CsvField(t.Title),
                t.Status,
                t.Priority,
                CsvField(t.AssignedToUserName ?? ""),
                t.DueDate?.ToString("s") ?? "",
                t.CreatedAt.ToString("s"),
                t.UpdatedAt.ToString("s")));
        }

        return Content(sb.ToString(), "text/csv", System.Text.Encoding.UTF8);
    }

    private static string CsvField(string value) =>
        "\"" + value.Replace("\"", "\"\"") + "\"";
}