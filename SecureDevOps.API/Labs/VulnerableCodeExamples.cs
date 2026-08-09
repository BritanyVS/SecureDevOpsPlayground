using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace SecureDevOps.API.Labs;

/// <summary>
/// Catálogo de ejemplos de código inseguro utilizado exclusivamente
/// para pruebas de Snyk Code / SAST.
///
/// IMPORTANTE:
/// Estos métodos NO deben utilizarse en código de producción.
/// El objetivo es demostrar cómo una herramienta SAST identifica
/// patrones de implementación potencialmente vulnerables.
/// </summary>
public class VulnerableCodeExamples
{
    // ============================================================
    // 01 - HARDCODED CREDENTIALS
    // ============================================================

    /// <summary>
    /// Ejemplo de credencial almacenada directamente en el código.
    /// Snyk Code debería identificar este patrón.
    /// </summary>
    public string GetHardcodedPassword()
    {
        string password = "AdminPassword123!";

        return password;
    }


    // ============================================================
    // 02 - HARDCODED API KEY / SECRET
    // ============================================================

    /// <summary>
    /// Ejemplo de secreto/API key almacenado directamente
    /// en el código fuente.
    /// </summary>
    public string GetHardcodedApiKey()
    {
        string apiKey = "sk_test_1234567890_example_secret";

        return apiKey;
    }


    // ============================================================
    // 03 - WEAK CRYPTOGRAPHY - MD5
    // ============================================================

    /// <summary>
    /// MD5 no debe utilizarse para nuevas aplicaciones
    /// cuando se necesita una función hash criptográficamente segura.
    /// </summary>
    public string WeakHashMd5(string input)
    {
        using var md5 = MD5.Create();

        byte[] data = Encoding.UTF8.GetBytes(input);

        byte[] hash = md5.ComputeHash(data);

        return Convert.ToHexString(hash);
    }


    // ============================================================
    // 04 - WEAK CRYPTOGRAPHY - SHA1
    // ============================================================

    /// <summary>
    /// Ejemplo de utilización de SHA-1.
    /// </summary>
    public string WeakHashSha1(string input)
    {
        using var sha1 = SHA1.Create();

        byte[] data = Encoding.UTF8.GetBytes(input);

        byte[] hash = sha1.ComputeHash(data);

        return Convert.ToHexString(hash);
    }


    // ============================================================
    // 05 - INSECURE RANDOMNESS
    // ============================================================

    /// <summary>
    /// System.Random no está diseñado para generar valores
    /// criptográficamente seguros.
    /// </summary>
    public string GenerateInsecureToken()
    {
        Random random = new Random();

        return random.Next(100000, 999999).ToString();
    }


    // ============================================================
    // 06 - PATH TRAVERSAL
    // ============================================================

    /// <summary>
    /// El nombre del archivo se utiliza directamente para acceder
    /// al sistema de archivos.
    ///
    /// En una aplicación real, fileName podría provenir de una
    /// entrada controlada por el usuario.
    /// </summary>
    public string ReadUserFile(string fileName)
    {
        return File.ReadAllText(fileName);
    }


    // ============================================================
    // 07 - COMMAND INJECTION
    // ============================================================

    /// <summary>
    /// Ejemplo de construcción de un comando del sistema utilizando
    /// una entrada externa.
    ///
    /// NO ejecutar con datos proporcionados por usuarios reales.
    /// </summary>
    public void ExecuteSystemCommand(string command)
    {
        Process.Start("cmd.exe", "/c " + command);
    }


    // ============================================================
    // 08 - COMMAND INJECTION / SHELL EXECUTION
    // ============================================================

    /// <summary>
    /// Segundo ejemplo de ejecución de procesos con datos externos.
    /// </summary>
    public void ExecuteProcess(string executable, string arguments)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = true
        };

        Process.Start(processInfo);
    }


    // ============================================================
    // 09 - SSRF
    // ============================================================

    /// <summary>
    /// La aplicación realiza una petición HTTP utilizando una URL
    /// recibida externamente sin validar adecuadamente el destino.
    /// </summary>
    public async Task<string> FetchExternalResource(
        string url)
    {
        using HttpClient client = new HttpClient();

        return await client.GetStringAsync(url);
    }


    // ============================================================
    // 10 - SSRF CON HttpClient
    // ============================================================

    /// <summary>
    /// Otro ejemplo de solicitud a una URL proporcionada
    /// directamente por una fuente externa.
    /// </summary>
    public async Task<string> DownloadResource(string userProvidedUrl)
    {
        using HttpClient client = new HttpClient();

        HttpResponseMessage response =
            await client.GetAsync(userProvidedUrl);

        return await response.Content.ReadAsStringAsync();
    }


    // ============================================================
    // 11 - INSECURE DESERIALIZATION
    // ============================================================

    /// <summary>
    /// Ejemplo conceptual de deserialización de datos externos.
    ///
    /// Se utiliza como caso de estudio para analizar cómo SAST
    /// identifica operaciones de deserialización inseguras.
    /// </summary>
    public T DeserializeData<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json)!;
    }


    // ============================================================
    // 12 - XML EXTERNAL ENTITY / UNSAFE XML PARSING
    // ============================================================

    /// <summary>
    /// Ejemplo de parser XML configurado de forma insegura.
    /// </summary>
    public XmlDocument ParseXmlUnsafe(string xml)
    {
        XmlDocument document = new XmlDocument();

        document.XmlResolver = new XmlUrlResolver();

        document.LoadXml(xml);

        return document;
    }


    // ============================================================
    // 13 - SENSITIVE DATA EXPOSURE
    // ============================================================

    /// <summary>
    /// Ejemplo de información sensible enviada a un log.
    /// </summary>
    public void LogSensitiveInformation(
        string username,
        string password,
        string creditCard)
    {
        Console.WriteLine(
            $"Username: {username}, " +
            $"Password: {password}, " +
            $"CreditCard: {creditCard}");
    }


    // ============================================================
    // 14 - SENSITIVE DATA IN EXCEPTION
    // ============================================================

    /// <summary>
    /// Ejemplo de exposición de información sensible mediante
    /// mensajes de excepción.
    /// </summary>
    public string ProcessUserData(
        string username,
        string password)
    {
        try
        {
            // Código de demostración.
            throw new Exception(
                $"Authentication failed for " +
                $"{username} with password {password}");
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }


    // ============================================================
    // 15 - SQL INJECTION - EJEMPLO CON SQL DIRECTO
    // ============================================================

    /// <summary>
    /// EJEMPLO EDUCATIVO.
    ///
    /// Construir SQL concatenando directamente una entrada externa
    /// puede permitir SQL Injection.
    ///
    /// No se conecta a la base de datos real del laboratorio.
    /// </summary>
    public string BuildUnsafeSqlQuery(string username)
    {
        string query =
            "SELECT * FROM Users WHERE Username = '" +
            username +
            "'";

        return query;
    }


    // ============================================================
    // 16 - SQL INJECTION - STRING INTERPOLATION
    // ============================================================

    /// <summary>
    /// Segundo ejemplo de construcción insegura de SQL.
    /// </summary>
    public string BuildUnsafeSqlQueryInterpolated(
        string username)
    {
        return $"SELECT * FROM Users WHERE Username = '{username}'";
    }


    // ============================================================
    // 17 - OPEN REDIRECT
    // ============================================================

    /// <summary>
    /// Ejemplo conceptual de redirección basada directamente
    /// en una URL externa.
    /// </summary>
    public string GetRedirectUrl(string url)
    {
        return url;
    }


    // ============================================================
    // 18 - WEAK PASSWORD HASHING
    // ============================================================

    /// <summary>
    /// Ejemplo de uso de MD5 para almacenar contraseñas.
    ///
    /// Las contraseñas deben utilizar algoritmos especializados
    /// como BCrypt, Argon2 o PBKDF2.
    /// </summary>
    public string HashPasswordWeakly(string password)
    {
        using var md5 = MD5.Create();

        byte[] bytes =
            Encoding.UTF8.GetBytes(password);

        byte[] hash =
            md5.ComputeHash(bytes);

        return Convert.ToHexString(hash);
    }


    // ============================================================
    // 19 - HARDCODED CONNECTION STRING
    // ============================================================

    /// <summary>
    /// Ejemplo de cadena de conexión que contiene credenciales.
    /// </summary>
    public string GetConnectionString()
    {
        return
            "Server=localhost;" +
            "Database=SecureDevOps;" +
            "User Id=admin;" +
            "Password=AdminPassword123!";
    }


    // ============================================================
    // 20 - INSECURE FILE PATH CONSTRUCCIÓN
    // ============================================================

    /// <summary>
    /// Construcción directa de una ruta utilizando entrada externa.
    /// </summary>
    public string BuildFilePath(string fileName)
    {
        string baseDirectory =
            @"C:\SecureDevOps\Files\";

        return baseDirectory + fileName;
    }


    // ============================================================
    // 21 - POTENTIAL XSS / HTML INJECTION
    // ============================================================

    /// <summary>
    /// Ejemplo conceptual de contenido proporcionado por el usuario
    /// que posteriormente podría ser renderizado como HTML sin
    /// codificación.
    /// </summary>
    public string BuildHtml(string userInput)
    {
        return $"<html><body>{userInput}</body></html>";
    }


    // ============================================================
    // 22 - DEBUG INFORMATION EXPOSURE
    // ============================================================

    /// <summary>
    /// Ejemplo de exposición innecesaria de información interna.
    /// </summary>
    public string GetDebugInformation(
        string username)
    {
        return
            $"User: {username}\n" +
            $"Machine: {Environment.MachineName}\n" +
            $"Directory: {Environment.CurrentDirectory}\n" +
            $"OS: {Environment.OSVersion}";
    }


    // ============================================================
    // 23 - SENSITIVE INFORMATION IN LOG
    // ============================================================

    /// <summary>
    /// Ejemplo de registro de token sensible.
    /// </summary>
    public void LogToken(string token)
    {
        Console.WriteLine(
            $"Authentication token: {token}");
    }


    // ============================================================
    // 24 - UNSAFE URL DOWNLOAD
    // ============================================================

    /// <summary>
    /// Descarga de contenido desde una URL proporcionada
    /// directamente por una fuente externa.
    /// </summary>
    public async Task<byte[]> DownloadFile(
        string url)
    {
        using HttpClient client = new HttpClient();

        return await client.GetByteArrayAsync(url);
    }


    // ============================================================
    // 25 - WEAK SECURITY CONFIGURATION
    // ============================================================

    /// <summary>
    /// Ejemplo conceptual de configuración insegura.
    /// </summary>
    public bool IsSecurityDisabled()
    {
        bool disableSecurity = true;

        return disableSecurity;
    }
}