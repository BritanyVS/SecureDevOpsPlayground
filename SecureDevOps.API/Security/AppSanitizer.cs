using System.Text.Encodings.Web;

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
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return HtmlEncoder.Default.Encode(input);
    }
}
