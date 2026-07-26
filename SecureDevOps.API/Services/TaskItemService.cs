using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;
using SecureDevOps.API.DTOs.TaskItem;
using SecureDevOps.API.Models;

namespace SecureDevOps.API.Services;

public class TaskItemService : ITaskItemService
{
    private readonly AppDbContext _context;

    public TaskItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItemResponseDto>> GetAllAsync()
    {
        var tasks = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .ToListAsync();

        return tasks.Select(MapToResponseDto);
    }

    public async Task<TaskItemResponseDto?> GetByIdAsync(Guid id)
    {
        var task = await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? null : MapToResponseDto(task);
    }

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

    public async Task<TaskItemResponseDto?> UpdateAsync(Guid id, TaskItemUpdateDto dto)
    {
        var task = await _context.TaskItems.FindAsync(id);
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.TaskItems.FindAsync(id);
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
