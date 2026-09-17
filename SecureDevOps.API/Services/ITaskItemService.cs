using SecureDevOps.API.DTOs.TaskItem;

namespace SecureDevOps.API.Services;

// ⚠️ LABORATORIO INTENCIONALMENTE VULNERABLE (Snyk Code → Broken Access Control / IDOR).
// La versión "segura" filtra cada consulta por el usuario del token (ClaimTypes.NameIdentifier).
public interface ITaskItemService
{
    Task<IEnumerable<TaskItemResponseDto>> GetAllAsync();
    Task<TaskItemResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItemResponseDto>> SearchAsync(string query);
    Task<TaskItemResponseDto> CreateAsync(TaskItemCreateDto dto);
    Task<TaskItemResponseDto?> UpdateAsync(Guid id, TaskItemUpdateDto dto);
    Task<TaskItemResponseDto?> UpdateStatusAsync(Guid id, TaskStatusUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<TaskCommentResponseDto>> ListCommentsAsync(Guid taskId);
    Task<TaskCommentResponseDto?> CreateCommentAsync(Guid taskId, TaskCommentCreateDto dto);
    Task<IEnumerable<TaskItemResponseDto>> ExportTasksAsync();
}