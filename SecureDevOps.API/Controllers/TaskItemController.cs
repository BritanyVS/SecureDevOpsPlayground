using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDevOps.API.DTOs.TaskItem;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Controllers;

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

    // El userId se obtiene del token autenticado, NO de la petición del cliente.
    // Esto garantiza que un usuario solo pueda acceder a sus propias tareas.
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemResponseDto>>> GetAll()
    {
        var tasks = await _service.GetAllAsync(CurrentUserId);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItemResponseDto>> GetById(Guid id)
    {
        var task = await _service.GetByIdAsync(id, CurrentUserId);
        if (task is null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponseDto>> Create(TaskItemCreateDto dto)
    {
        var created = await _service.CreateAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskItemResponseDto>> Update(Guid id, TaskItemUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto, CurrentUserId);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id, CurrentUserId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}