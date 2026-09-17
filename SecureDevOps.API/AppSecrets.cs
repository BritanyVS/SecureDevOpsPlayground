namespace SecureDevOps.API;

// ⚠️ LABORATORIO INTENCIONALMENTE VULNERABLE (Snyk Code → Hardcoded credentials).
// Secretos literales en el código. FIX: leer de variables de entorno / secret manager.
public static class AppSecrets
{
    public const string AwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
    public const string AwsSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
    public const string DbPassword = "P@ssw0rd_Demo_123!";
    public const string JwtSecret = "SuperSecretJwtKeyHardcoded_Demo_123!";
}