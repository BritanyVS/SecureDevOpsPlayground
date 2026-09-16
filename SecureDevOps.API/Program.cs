using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureDevOps.API.Data;
using SecureDevOps.API.Models;
using SecureDevOps.API.Models.Enums;
using SecureDevOps.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Desactivar file watching en config JSON (resuelve inotify limit en Render free)
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskItemService, TaskItemService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// JWT secret: la variable de entorno JWT_SECRET tiene prioridad sobre appsettings.json.
// En producción NUNCA debe quedar el valor por defecto de appsettings.
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT_SECRET must be configured (env var or appsettings Jwt:SecretKey).");
}
builder.Configuration["Jwt:SecretKey"] = jwtSecret;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
    };
});

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var isVercel = origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
            var isAllowed = allowedOrigins.Contains(origin);
            return isVercel || isAllowed;
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed usuarios de demo (Render free tier borra la DB en cada restart)
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var demoUsers = new (string email, string password, string username, string role)[]
    {
        ("juan@gmail.com", "contra1234", "Juan", "Admin"),
        ("prueba@gmail.com", "contra1234", "Prueba", "User"),
        ("admin@gmail.com", "Admin123!", "Admin", "Admin")
    };

    foreach (var (email, password, username, role) in demoUsers)
    {
        if (!db.Users.Any(u => u.Email == email))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                FirstName = username.Split(' ')[0],
                PasswordHash = hasher.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
    db.SaveChanges();

    // Seed tareas de demo para que el DAST tenga datos en /api/tasks
    var juan = db.Users.FirstOrDefault(u => u.Email == "juan@gmail.com");
    if (juan != null && !db.TaskItems.Any())
    {
        var demoTasks = new[]
        {
            "Configurar pipeline CI/CD",
            "Revisar vulnerabilidad en dependencies",
            "Documentar políticas de seguridad",
            "Implementar autenticación multifactor"
        };
        foreach (var title in demoTasks)
        {
            db.TaskItems.Add(new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                Status = TaskItemStatus.Pending,
                Priority = TaskPriority.Medium,
                CreatedByUserId = juan.Id,
                AssignedToUserId = juan.Id,
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                           Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check para orquestadores / docker-compose (no requiere auth).
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.Run();
