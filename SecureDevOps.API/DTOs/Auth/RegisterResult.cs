namespace SecureDevOps.API.DTOs.Auth;

public class RegisterResult
{
    public bool IsSuccess { get; set; }
    public UserResponseDto? User { get; set; }
    public List<string> Errors { get; set; } = new();
}
