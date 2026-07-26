using SecureDevOps.API.Models;

namespace SecureDevOps.API.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
