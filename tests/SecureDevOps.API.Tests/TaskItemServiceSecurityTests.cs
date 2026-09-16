using SecureDevOps.API.DTOs.TaskItem;
using SecureDevOps.API.Services;

namespace SecureDevOps.API.Tests;

/// <summary>
/// Tests de AISLAMIENTO DE USUARIOS (Broken Object Level Authorization / IDOR).
/// Un usuario NUNCA debe poder ver, modificar o eliminar tareas de otros usuarios.
/// </summary>
public class TaskItemServiceSecurityTests
{
    private static TaskItemService CreateService(Data.AppDbContext context) => new(context);

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUsersTasks()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");
        var bob = await TestDbContextFactory.CreateUserAsync(db, "bob@test.com", "bob");

        await TestDbContextFactory.CreateTaskAsync(db, alice.Id, "Alice task");
        await TestDbContextFactory.CreateTaskAsync(db, bob.Id, "Bob task");
        await TestDbContextFactory.CreateTaskAsync(db, bob.Id, "Bob task 2");

        var service = CreateService(db);
        var aliceTasks = (await service.GetAllAsync(alice.Id)).ToList();

        Assert.Single(aliceTasks);
        Assert.Equal("Alice task", aliceTasks[0].Title);
        Assert.All(aliceTasks, t => Assert.Equal(alice.Id, t.CreatedByUserId));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForAnotherUsersTask()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");
        var bob = await TestDbContextFactory.CreateUserAsync(db, "bob@test.com", "bob");
        var bobTask = await TestDbContextFactory.CreateTaskAsync(db, bob.Id, "Bob secret task");

        var service = CreateService(db);

        Assert.Null(await service.GetByIdAsync(bobTask.Id, alice.Id));
        Assert.NotNull(await service.GetByIdAsync(bobTask.Id, bob.Id));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullAndDoesNotModifyAnotherUsersTask()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");
        var bob = await TestDbContextFactory.CreateUserAsync(db, "bob@test.com", "bob");
        var bobTask = await TestDbContextFactory.CreateTaskAsync(db, bob.Id, "Bob task");

        var service = CreateService(db);
        var update = new TaskItemUpdateDto
        {
            Title = "HACKED by alice",
            Status = Models.Enums.TaskItemStatus.Completed,
            Priority = Models.Enums.TaskPriority.High
        };

        var result = await service.UpdateAsync(bobTask.Id, update, alice.Id);

        Assert.Null(result);
        var after = await service.GetByIdAsync(bobTask.Id, bob.Id);
        Assert.NotNull(after);
        Assert.Equal("Bob task", after!.Title);
        Assert.Equal(Models.Enums.TaskItemStatus.Pending, after.Status);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseAndKeepsAnotherUsersTask()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");
        var bob = await TestDbContextFactory.CreateUserAsync(db, "bob@test.com", "bob");
        var bobTask = await TestDbContextFactory.CreateTaskAsync(db, bob.Id, "Bob task");

        var service = CreateService(db);

        Assert.False(await service.DeleteAsync(bobTask.Id, alice.Id));
        Assert.True(await service.GetByIdAsync(bobTask.Id, bob.Id) is not null);
    }

    [Fact]
    public async Task CreateAsync_ForcesOwnerToCurrentUserIgnoringClientValue()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");
        var bob = await TestDbContextFactory.CreateUserAsync(db, "bob@test.com", "bob");

        var service = CreateService(db);
        var dto = new TaskItemCreateDto
        {
            Title = "Spoofed owner attack",
            Description = "Intenta crear una tarea en nombre de otro usuario",
            // createdByUserId del cliente apunta a Bob, pero el backend debe usar a Alice:
            // el DTO incluso lo podemos omitir; el servicio SIEMPRE usa currentUserId.
            AssignedToUserId = bob.Id
        };

        var created = await service.CreateAsync(dto, alice.Id);

        Assert.Equal(alice.Id, created.CreatedByUserId);
        Assert.NotEqual(bob.Id, created.CreatedByUserId);
    }

    [Fact]
    public async Task CreateAsync_RespectsUserOwnerForList()
    {
        await using var db = TestDbContextFactory.Create();
        var alice = await TestDbContextFactory.CreateUserAsync(db, "alice@test.com", "alice");

        var service = CreateService(db);
        var dto = new TaskItemCreateDto { Title = "Mi tarea", AssignedToUserId = alice.Id };
        await service.CreateAsync(dto, alice.Id);

        var tasks = (await service.GetAllAsync(alice.Id)).ToList();
        Assert.Single(tasks);
    }
}