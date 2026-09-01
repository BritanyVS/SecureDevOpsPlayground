namespace SecureDevOps.API.Services;

// ⚠️ LABORATORIO: Errores de seguridad intencionales para Snyk Code.

public class LabVulnerabilitiesService
{
    // VULNERABILIDAD: password hasheada con criptografía débil (MD5)
    // Snyk Code lo detecta como "Insecure Cryptographic hash function"
    public string HashWithMd5(string password)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    // VULNERABILIDAD: secreto hardcodeado en el código fuente
    // Snyk Code lo reporta como "hardcoded secret"
    private const string ApiSecret = "AKIAIOSFODNN7EXAMPLE-wJalrXUtnFEMI-K7MDENG";

    // VULNERABILIDAD: se expone stack trace / info sensible en respuestas públicas
    public object HandleRequest(string input)
    {
        try
        {
            // FALLO Snyk Code: input del usuario usado para construir HTML sin escapar (XSS)
            return new { html = "<div>" + input + "</div>", apiKey = ApiSecret };
        }
        catch (Exception ex)
        {
            // VULNERABILIDAD: logueo información sensible en excepciones
            System.Console.WriteLine("ERROR:" + ex.ToString() + " INPUT:" + input);
            return new { error = ex.ToString() };
        }
    }

    // VULNERABILIDAD: SQL Injection - input del usuario concatenado en consultas
    public string BuildSqlQuery(string tableName, string userInput)
    {
        return $"SELECT * FROM {tableName} WHERE name = '{userInput}'";
    }

    // VULNERABILIDAD: path traversal / acceso a archivos no validado
    public string ReadFile(string fileName)
    {
        var path = System.IO.Path.Combine("C:\\uploads\\", fileName);
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : string.Empty;
    }
}