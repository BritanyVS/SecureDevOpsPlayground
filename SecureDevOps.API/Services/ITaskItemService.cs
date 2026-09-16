using SecureDevOps.API.DTOs.TaskItem;

namespace SecureDevOps.API.Services;

public interface ITaskItemService
{
    // El userId siempre se obtiene del token autenticado (claims), nunca de la petición del cliente.
    // Esto garantiza aislamiento de datos entre usuarios (previene Broken Object Level Authorization / IDOR).
    Task<IEnumerable<TaskItemResponseDto>> GetAllAsync(Guid currentUserId);
    Task<TaskItemResponseDto?> GetByIdAsync(Guid id, Guid currentUserId);
    Task<TaskItemResponseDto> CreateAsync(TaskItemCreateDto dto, Guid currentUserId);
    Task<TaskItemResponseDto?> UpdateAsync(Guid id, TaskItemUpdateDto dto, Guid currentUserId);
    Task<bool> DeleteAsync(Guid id, Guid currentUserId);
}
