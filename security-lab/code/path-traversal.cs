namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: PATH TRAVERSAL
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class PathTraversalLab
{
    private readonly string _baseDir = "C:\\app\\uploads";

    // VULNERABLE: combinación de path sin validar el resultado.
    public string ReadFile(string fileName)
    {
        // FALLO: fileName = "../../../Windows/system32/config/SAM" → lee fuera de _baseDir.
        var path = Path.Combine(_baseDir, fileName);
        return File.ReadAllText(path);
    }

    // ✅ CORREGIDO: se normaliza el path y se valida que el resultado quede dentro de _baseDir.
    public string ReadFileSecure(string fileName)
    {
        var baseFull = Path.GetFullPath(_baseDir);
        var target = Path.GetFullPath(Path.Combine(_baseDir, fileName));

        if (!target.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Path escapes the allowed directory.");
        }

        return File.ReadAllText(target);
    }
}