using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureDevOps.API.Models;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Tests;

public class JwtServiceTests
{
    private const string Secret = "test-secret-key-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private static IConfiguration BuildConfig()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = Secret,
            ["Jwt:Issuer"] = "SecureDevOps.API",
            ["Jwt:Audience"] = "SecureDevOpsPlayground",
            ["Jwt:ExpirationInMinutes"] = "30"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static User CreateUser(Guid id) => new()
    {
        Id = id,
        Username = "juan",
        Email = "juan@test.com",
        Role = "User",
        IsActive = true
    };

    [Fact]
    public void GenerateToken_IncludesIdentityClaims()
    {
        var userId = Guid.NewGuid();
        var service = new JwtService(BuildConfig());

        var token = service.GenerateToken(CreateUser(userId));
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("juan@test.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("User", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_CanBeValidatedWithSameKey_IssuerAudienceLifetime()
    {
        var service = new JwtService(BuildConfig());
        var user = CreateUser(Guid.NewGuid());

        var token = service.GenerateToken(user);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "SecureDevOps.API",
            ValidAudience = "SecureDevOpsPlayground",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, parameters, out _);

        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}