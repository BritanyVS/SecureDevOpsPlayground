namespace SecureDevOps.API.Security;

// Sanitizer interno del laboratorio (demo Snyk Code Rule Extensions).
// Snyk Code NO lo reconoce por defecto -> genera falso positivo hasta
// registrarlo como custom sanitizer Flow Through:
//   FQN: global::SecureDevOps.API.Security.AppSanitizer.SanitizeHtml
//   Rules: Reflected XSS + Stored XSS (C#)
// Ver docs/VULNERABILITIES.md
public static class AppSanitizer
{
    // Flow Through: el valor de retorno siempre sale sanitizado,
    // aunque el input venga contaminado.
    // NOTA DEMO: implementación propia a propósito (replace manual) para que
    // Snyk Code NO la reconozca como sanitizer conocido. Así genera el falso
    // positivo que luego suprime la Rule Extension. No usar HtmlEncoder aquí
    // porque Snyk ya lo conoce y el test daría "No findings affected".
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input
            .Replace("<", "[lt]")
            .Replace(">", "[gt]")
            .Replace("\"", "[quot]")
            .Replace("'", "[apos]")
            .Replace("&", "[amp]");
    }
}
