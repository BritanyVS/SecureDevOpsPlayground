using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecureDevOps.API.DTOs.Auth;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Tests;

public class AuthServiceTests
{
    private static AuthService CreateAuthService(Data.AppDbContext db)
    {
        var configuration = BuildJwtConfig();
        var jwtService = new JwtService(configuration);
        var hasher = new PasswordHasherService();
        return new AuthService(db, hasher, jwtService);
    }

    private static IConfiguration BuildJwtConfig()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            ["Jwt:Issuer"] = "SecureDevOps.API",
            ["Jwt:Audience"] = "SecureDevOpsPlayground",
            ["Jwt:ExpirationInMinutes"] = "30"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccessAndHashesPassword()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateAuthService(db);

        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Contrato123!",
            FirstName = "New",
            LastName = "User"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.User);
        Assert.Empty(result.Errors);

        var stored = await db.Users.FirstOrDefaultAsync(u => u.Email == "newuser@test.com");
        Assert.NotNull(stored);
        Assert.NotEqual("Contrato123!", stored!.PasswordHash);          // nunca en texto plano
        Assert.StartsWith("$2", stored.PasswordHash);                   // formato BCrypt
    }

    [Fact]
    public async Task RegisterAsync_FailsOnDuplicateEmail()
    {
        await using var db = TestDbContextFactory.Create();
        await TestDbContextFactory.CreateUserAsync(db, "dup@test.com", "dupuser");

        var service = CreateAuthService(db);
        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "otheruser",
            Email = "dup@test.com",
            Password = "Contrato123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Email already exists", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenForValidCredentials()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateAuthService(db);
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "loginuser",
            Email = "login@test.com",
            Password = "Contrato123!"
        });

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Email = "login@test.com",
            Password = "Contrato123!"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result!.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullForWrongPassword()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateAuthService(db);
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "loginuser2",
            Email = "login2@test.com",
            Password = "Contrato123!"
        });

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Email = "login2@test.com",
            Password = "WrongPass123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullForUnknownEmail()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateAuthService(db);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Email = "ghost@test.com",
            Password = "Whatever123!"
        });

        Assert.Null(result);
    }
}