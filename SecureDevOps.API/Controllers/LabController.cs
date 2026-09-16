using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SecureDevOps.API.Controllers;

// ⚠️ LABORATORIO: Endpoints intencionalmente vulnerables para Snyk API & Web (DAST).
// Estos endpoints existen para que el escáner pueda detectar las vulnerabilidades
// a través de HTTP. NO usar en producción.
//
// ESTÁN DESACTIVADOS POR DEFECTO. Para habilitarlos en un entorno de laboratorio:
//   appsettings.json → "SecurityLab": { "Enabled": true }
//   o variable de entorno → SecurityLab__Enabled=true
// Con el lab desactivado, cada endpoint responde 404 sin exponer detalle.

[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly bool _labEnabled;

    public LabController(IConfiguration config)
    {
        _config = config;
        _labEnabled = config.GetValue<bool>("SecurityLab:Enabled");
    }

    private IActionResult LabDisabledResult() =>
        NotFound("Lab endpoints are disabled. Set SecurityLab__Enabled=true to run the Snyk DAST demo.");

    // VULNERABILIDAD: Reflected XSS - input del usuario se devuelve sin escapar en HTML
    // GET /api/lab/xss?input=<script>alert(1)</script>
    [HttpGet("xss")]
    public ContentResult Xss([FromQuery] string input)
    {
        if (!_labEnabled) return LabDisabledResult() as ContentResult ?? Content("", "text/plain");
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
        if (!_labEnabled) return LabDisabledResult();
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
        if (!_labEnabled) return LabDisabledResult();
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
        if (!_labEnabled) return LabDisabledResult();
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

    // VULNERABILIDAD: SSRF - el servidor pide URLs controladas por el usuario
    // GET /api/lab/fetch?url=http://169.254.169.254/latest/meta-data/
    [HttpGet("fetch")]
    public async Task<IActionResult> FetchUrl([FromQuery] string url)
    {
        if (!_labEnabled) return LabDisabledResult();
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return Content(response, "text/plain");
    }

    // VULNERABILIDAD: Open Redirect - redirige a cualquier URL controlada por el usuario
    // GET /api/lab/redirect?url=https://evil.com
    [HttpGet("redirect")]
    public IActionResult OpenRedirect([FromQuery] string url)
    {
        if (!_labEnabled) return LabDisabledResult();
        return Redirect(url);
    }

    // VULNERABILIDAD: secrets expuestos en una respuesta pública (hardcoded)
    // GET /api/lab/secret
    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        if (!_labEnabled) return LabDisabledResult();
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