using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SecureDevOps.API.Controllers;

// ⚠️ LABORATORIO: Endpoints intencionalmente vulnerables para Snyk API & Web (DAST).
// Estos endpoints existen para que el escáner pueda detectar las vulnerabilidades
// a través de HTTP. NO usar en producción.

[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly IConfiguration _config;

    public LabController(IConfiguration config)
    {
        _config = config;
    }

    // VULNERABILIDAD: Reflected XSS - input del usuario se devuelve sin escapar en HTML
    // GET /api/lab/xss?input=<script>alert(1)</script>
    [HttpGet("xss")]
    public ContentResult Xss([FromQuery] string input)
    {
        var html = $@"
<html>
<body>
<h1>Reflected XSS Lab</h1>
<p>Input recibido:</p>
<div>{input}</div>
</body>
</html>";
        return Content(html, "text/html");
    }

    // VULNERABILIDAD: SQL Injection - input concatenado en consulta SQL
    // GET /api/lab/search?q=abc' UNION SELECT ...--
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q)
    {
        var connString = _config.GetConnectionString("DefaultConnection")!;
        using var connection = new SqliteConnection(connString);
        connection.Open();

        var query = $"SELECT Username FROM Users WHERE Username LIKE '%{q}%'";

        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        var results = new List<string>();
        while (reader.Read())
        {
            results.Add(reader.GetString(0));
        }
        return Ok(new { query, results });
    }

    // VULNERABILIDAD: Path Traversal - acceso a archivos sin validar
    // GET /api/lab/file?name=../../etc/passwd
    [HttpGet("file")]
    public IActionResult ReadFile([FromQuery] string name)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var path = Path.Combine(uploadsDir, name);
        if (!System.IO.File.Exists(path))
        {
            return NotFound($"Archivo no existe: {path}");
        }

        return Content(System.IO.File.ReadAllText(path), "text/plain");
    }

    // VULNERABILIDAD: Command Injection - ejecución de comandos sin validar
    // GET /api/lab/ping?host=localhost;whoami
    [HttpGet("ping")]
    public IActionResult Ping([FromQuery] string host)
    {
        var psi = new ProcessStartInfo("/bin/sh", $"-c \"ping -c 4 {host}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        try
        {
            using var process = Process.Start(psi);
            process!.WaitForExit(5000);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            return Content((output + error).Trim(), "text/plain");
        }
        catch (Exception ex)
        {
            return Ok($"Error: {ex.Message}");
        }
    }

    // VULNERABILIDAD: SSRF - el servidor pide URLs controladas por el usuario
    // GET /api/lab/fetch?url=http://169.254.169.254/latest/meta-data/
    [HttpGet("fetch")]
    public async Task<IActionResult> FetchUrl([FromQuery] string url)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return Content(response, "text/plain");
    }

    // VULNERABILIDAD: secrets expuestos en una respuesta pública (hardcoded)
    // GET /api/lab/secret
    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        return Ok(new
        {
            apiKey = "AKIAIOSFODNN7EXAMPLE-wJalrXUtnFEMI-K7MDENG",
            awsSecret = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            dbPassword = "P@ssw0rd_Fake_123",
            stripeToken = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                ?? "sk_test_" + "4eC39HqLyjWDarjtT1zdp7dc"
        });
    }
}