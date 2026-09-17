namespace SecureDevOps.API.DTOs.TaskItem;

public class TaskCommentResponseDto
{
    public Guid Id { get; set; }

    public Guid TaskItemId { get; set; }

    public Guid? AuthorUserId { get; set; }

    public string? AuthorUsername { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}