using System.Diagnostics;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: COMMAND INJECTION
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class CommandInjectionLab
{
    // VULNERABLE: se ejecuta el input del usuario como parte de un comando del shell.
    public string PingHost(string host)
    {
        var psi = new ProcessStartInfo("/bin/sh", $"-c \"ping -c 1 {host}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        // FALLO: host no se valida → `; rm -rf /` o `| cat /etc/passwd` se ejecuta.
        using var process = Process.Start(psi)!;
        process.WaitForExit(5000);
        return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
    }

    // ✅ CORREGIDO: ArgumentList separa el ejecutable de los argumentos.
    // El shell no interpreta el input → no hay inyección.
    public string PingHostSecure(string host)
    {
        // Validación extra: host debe ser un nombre/dominio alfanumérico seguro.
        if (!System.Text.RegularExpressions.Regex.IsMatch(host, @"^[a-zA-Z0-9\.\-]+$"))
        {
            throw new ArgumentException("Invalid host");
        }

        var psi = new ProcessStartInfo("/usr/bin/ping")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("1");
        psi.ArgumentList.Add(host);

        using var process = Process.Start(psi)!;
        process.WaitForExit(5000);
        return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
    }
}