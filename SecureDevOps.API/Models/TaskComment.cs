using System.ComponentModel.DataAnnotations;

namespace SecureDevOps.API.Models;

public class TaskComment
{
    public Guid Id { get; set; }

    public Guid TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    // ⚠️ LABORATORIO: el autor viene del cliente (dto.AuthorUserId). FIX: token JWT.
    public Guid? AuthorUserId { get; set; }

    public string? AuthorUsername { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}