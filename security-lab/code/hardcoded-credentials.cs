using System.Text;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: HARDCODED CREDENTIALS / SECRETOS EN CÓDIGO
// Código INTENCIONALMENTE vulnerable para que Snyk Code (y Snyk Secrets) lo detecte.

public class HardcodedCredentialsLab
{
    // VULNERABLE: credenciales literales en el código fuente.
    private const string DbPassword = "P@ssw0rd123!";
    private static readonly string ApiKey = "AKIAIOSFODNN7EXAMPLE";      // formato AWS
    private static readonly string StripeKey = "sk_live_51FakeStripeKey123456789";
    private const string JwtSecret = "sup3rs3cr3t-jwt-key-0123456789abcdef";

    public string Connect()
    {
        // FALLO: si alguien ve el código (o Snyk), expone las credenciales.
        return $"Server=prod;Database=tasks;User Id=sa;Password={DbPassword}";
    }

    public string SignToken()
    {
        // FALLO: el JWT se firma con un secreto hardcodeado → forjar tokens.
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JwtSecret));
    }

    // ✅ CORREGIDO: las credenciales vienen de variables de entorno / secret manager.
    // Snyk Secrets/Code espera leer en runtime, nunca literales en el código.
    public string ConnectSecure()
    {
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
            ?? throw new InvalidOperationException("DB_PASSWORD not configured.");
        return $"Server=prod;Database=tasks;User Id=sa;Password={dbPassword}";
    }

    public string SignTokenSecure()
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET not configured.");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jwtSecret));
    }
}