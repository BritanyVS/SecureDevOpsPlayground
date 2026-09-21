using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SecureDevOps.API.Security;

namespace SecureDevOps.API.Controllers;

// ⚠️ LABORATORIO de Snyk API & Web (DAST): endpoints intencionalmente vulnerables,
// siempre accesibles para que el escáner dinámico los encuentre por HTTP.
// NO usar en producción. Ver docs/VULNERABILITIES.md.
[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly IConfiguration _config;

    public LabController(IConfiguration config)
    {
        _config = config;
    }

    // VULN: Reflected XSS. GET /api/lab/xss?input=<script>alert(1)</script>
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

    // DEMO Rule Extensions: mismo endpoint pero pasando por el sanitizer interno.
    // Sin registrar el sanitizer en Snyk -> falso positivo (Snyk lo sigue marcando).
    // Con sanitizer Flow Through registrado -> el hallazgo desaparece.
    // GET /api/lab/xss-safe?input=<script>alert(1)</script>
    [HttpGet("xss-safe")]
    public ContentResult XssSafe([FromQuery] string input)
    {
        var safe = SecureDevOps.API.Security.AppSanitizer.SanitizeHtml(input);
        var html = $@"
<html>
<body>
<h1>Reflected XSS Lab (sanitizado)</h1>
<p>Input recibido:</p>
<div>{safe}</div>
</body>
</html>";
        return Content(html, "text/html");
    }

    // VULN: SQL Injection. GET /api/lab/search?q=abc' UNION SELECT ...
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

    // VULN: Path Traversal. GET /api/lab/file?name=../../etc/passwd
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

    // VULN: Command Injection. GET /api/lab/ping?host=localhost;whoami
    [HttpGet("ping")]
    public IActionResult Ping([FromQuery] string host)
    {
        var psi = new ProcessStartInfo("/bin/sh", $"-c \"echo {host}\"")
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

    // VULN: SSRF. GET /api/lab/fetch?url=http://169.254.169.254/latest/meta-data/
    [HttpGet("fetch")]
    public async Task<IActionResult> FetchUrl([FromQuery] string url)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return Content(response, "text/plain");
    }

    // VULN: Open Redirect. GET /api/lab/redirect?url=https://evil.com
    [HttpGet("redirect")]
    public IActionResult OpenRedirect([FromQuery] string url)
    {
        return Redirect(url);
    }

    // VULN: secretos hardcodeados expuestos en una respuesta pública.
    // GET /api/lab/secret
    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        return Ok(new
        {
            apiKey = AppSecrets.AwsAccessKey,
            awsSecret = AppSecrets.AwsSecretKey,
            dbPassword = AppSecrets.DbPassword,
            jwtSecret = AppSecrets.JwtSecret
        });
    }
}