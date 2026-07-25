using Microsoft.EntityFrameworkCore;

namespace SecureDevOps.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
