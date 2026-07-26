using System.ComponentModel.DataAnnotations;
using SecureDevOps.API.Models.Enums;

namespace SecureDevOps.API.DTOs.TaskItem;

public class TaskItemUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }
}
