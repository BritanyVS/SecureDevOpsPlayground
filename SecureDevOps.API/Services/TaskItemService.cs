using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;
using SecureDevOps.API.DTOs.TaskItem;
using SecureDevOps.API.Models;

namespace SecureDevOps.API.Services;

// ⚠️ LABORATORIO INTENCIONALMENTE VULNERABLE.
// Vulnerabilidades pensadas (fáciles de resolver, ver docs/VULNERABILITIES.md):
//   [1] Broken Object Level Authorization (IDOR): no se comprueba que la tarea
//       pertenezca al usuario autenticado. FIX: filtrar por userId del token.
//   [2] SQL Injection: query se concatena en SQL crudo (FromSqlRaw). FIX: usar LINQ.
public class TaskItemService : ITaskItemService
{
    private readonly AppDbContext _context;

    public TaskItemService(AppDbContext context)
    {
        _context = context;
    }

    // [1] VULNERABLE: devuelve TODAS las tareas de todos los usuarios.
    // FIX: añadir .Where(t => t.CreatedByUserId == currentUserId).
    public async Task<IEnumerable<TaskItemResponseDto>> GetAllAsync()
    {
        var tasks = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .ToListAsync();

        return tasks.Select(MapToResponseDto);
    }

    // [1] VULNERABLE: cualquier usuario autenticado puede leer la tarea de otro por su id.
    // FIX: comprobar t.CreatedByUserId == currentUserId en el FirstOrDefault.
    public async Task<TaskItemResponseDto?> GetByIdAsync(Guid id)
    {
        var task = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? null : MapToResponseDto(task);
    }

    // [2] VULNERABLE: SQL Injection. query se concatena sin parametrizar.
    // GET /api/taskitem/search?q=abc' UNION SELECT ...
    // FIX: _context.TaskItems.Where(t => t.Title.Contains(query)).
    public async Task<IEnumerable<TaskItemResponseDto>> SearchAsync(string query)
    {
        var tasks = await _context.TaskItems
            .FromSqlRaw($"SELECT * FROM TaskItems WHERE Title LIKE '%{query}%'")
            .Include(t => t.AssignedToUser)
            .ToListAsync();

        return tasks.Select(MapToResponseDto);
    }

    // [1] VULNERABLE: el propietario VUENE DEL CLIENTE (dto.CreatedByUserId), no del token.
    // Snyk Code lo marca como "Improper Authorization" / "Hardcoded ID trust".
    public async Task<TaskItemResponseDto> CreateAsync(TaskItemCreateDto dto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            CreatedByUserId = dto.CreatedByUserId,
            AssignedToUserId = dto.AssignedToUserId,
            DueDate = dto.DueDate,
            Status = Models.Enums.TaskItemStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        var created = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .FirstAsync(t => t.Id == task.Id);

        return MapToResponseDto(created);
    }

    // [1] VULNERABLE: no se comprueba propiedad antes de editar.
    public async Task<TaskItemResponseDto?> UpdateAsync(Guid id, TaskItemUpdateDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return null;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.AssignedToUserId = dto.AssignedToUserId;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updated = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .FirstAsync(t => t.Id == task.Id);

        return MapToResponseDto(updated);
    }

    // [1] VULNERABLE: cualquier usuario puede borrar la tarea de otro.
    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    private static TaskItemResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskItemResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedByUserId = task.CreatedByUserId,
            AssignedToUserName = task.AssignedToUser?.Username,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}