using SecureDevOps.API.DTOs.Auth;

namespace SecureDevOps.API.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequestDto dto);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
}
