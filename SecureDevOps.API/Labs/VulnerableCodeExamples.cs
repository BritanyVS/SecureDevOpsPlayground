using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace SecureDevOps.API.Labs;

public class VulnerableCodeExamples
{
    /// <summary>
    /// Ejemplo de credencial almacenada directamente en el código.
    /// Snyk Code debería identificar este patrón.
    /// </summary>
    public string GetHardcodedPassword()
    {
        string password = "AdminPassword123!";

        return password;
    }


}