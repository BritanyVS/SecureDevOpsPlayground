using System.Text.RegularExpressions;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: UNSAFE INPUT HANDLING (validación / ReDoS / header injection)
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class UnsafeInputHandlingLab
{
    // VULNERABLE: regex catastrophically-backtracking (ReDoS → DoS del hilo).
    private static readonly Regex EvilRegex = new(@"^(a+)+$", RegexOptions.Compiled);

    public bool IsAStringOfAs(string input)
    {
        // FALLO: input = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaac"
        // → backtracking exponencial → CPU al 100%.
        return EvilRegex.IsMatch(input);
    }

    // ✅ CORREGIDO: regex lineal / timeout obligatorio.
    private static readonly Regex SafeRegex =
        new(@"^(a+)+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    public bool IsAStringOfAsSecure(string input)
    {
        return SafeRegex.IsMatch(input); // lanza RegexMatchTimeoutException en DoS
    }

    // VULNERABLE: datos del usuario en cabeceras sin validar (Header Injection).
    public string BuildLocationHeader(string url)
    {
        // FALLO: url = "/x\r\nSet-Cookie: evil=1" → inyección de cabeceras HTTP.
        return $"Location: {url}";
    }

    // ✅ CORREGIDO: validar el valor contra allowlist y eliminar CR/LF.
    public string BuildLocationHeaderSecure(string url)
    {
        var safe = url.Replace("\r", "").Replace("\n", "");
        if (!Uri.TryCreate(safe, UriKind.RelativeOrAbsolute, out _))
        {
            throw new ArgumentException("Invalid redirect target.");
        }
        return $"Location: {safe}";
    }
}