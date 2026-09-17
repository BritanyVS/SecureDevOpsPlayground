using System.ComponentModel.DataAnnotations;

namespace SecureDevOps.API.DTOs.TaskItem;

public class TaskCommentCreateDto
{
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    // ⚠️ LABORATORIO: el autor viene del cliente (vulnerable). FIX: del token JWT.
    public Guid? AuthorUserId { get; set; }
}