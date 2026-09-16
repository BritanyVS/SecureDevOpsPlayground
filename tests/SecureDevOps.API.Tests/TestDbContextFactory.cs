using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Data;
using SecureDevOps.API.Models;

namespace SecureDevOps.API.Tests;

/// <summary>
/// Factory de AppDbContext en memoria (InMemory provider).
/// Cada test usa un nombre de base de datos único para evitar contaminación entre tests.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"snyk-lab-tests-{Guid.NewGuid():N}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static async Task<User> CreateUserAsync(AppDbContext context, string email, string username, string role = "User")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            FirstName = username,
            PasswordHash = "fake-hash-not-used-in-tests",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<TaskItem> CreateTaskAsync(
        AppDbContext context,
        Guid ownerId,
        string title = "Sample task",
        Guid? assignedTo = null)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "desc",
            Status = Models.Enums.TaskItemStatus.Pending,
            Priority = Models.Enums.TaskPriority.Medium,
            CreatedByUserId = ownerId,
            AssignedToUserId = assignedTo ?? ownerId,
            DueDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();
        return task;
    }
}