using SecureDevOps.API.DTOs.TaskItem;

namespace SecureDevOps.API.Services;

public interface ITaskItemService
{
    Task<IEnumerable<TaskItemResponseDto>> GetAllAsync();
    Task<TaskItemResponseDto?> GetByIdAsync(Guid id);
    Task<TaskItemResponseDto> CreateAsync(TaskItemCreateDto dto);
    Task<TaskItemResponseDto?> UpdateAsync(Guid id, TaskItemUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
