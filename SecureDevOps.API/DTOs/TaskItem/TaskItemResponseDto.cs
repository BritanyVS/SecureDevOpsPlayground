using SecureDevOps.API.Models.Enums;

namespace SecureDevOps.API.DTOs.TaskItem;

public class TaskItemResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
