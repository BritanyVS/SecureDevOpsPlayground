using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;
using SecureDevOps.API.DTOs.Auth;
using SecureDevOps.API.Models;

namespace SecureDevOps.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        AppDbContext context,
        IPasswordHasherService passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequestDto dto)
    {
        var errors = new List<string>();

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            errors.Add("Email already exists");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            errors.Add("Username already exists");

        if (errors.Count > 0)
            return new RegisterResult { IsSuccess = false, Errors = errors };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResult
        {
            IsSuccess = true,
            User = MapToResponseDto(user)
        };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null)
            return null;

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
