using System.ComponentModel.DataAnnotations;
using SecureDevOps.API.Models.Enums;

namespace SecureDevOps.API.Models;

public class TaskItem
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }

    public User? AssignedToUser { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
