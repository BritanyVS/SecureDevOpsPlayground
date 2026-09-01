using System.Diagnostics;
using System.Net.Http;

namespace SnykLab.Code
{
    // ⚠️ LABORATORIO: Otras vulnerabilidades de código (Snyk Code)
    public class MiscVulnerabilities
    {
        // VULNERABILIDAD: Command Injection - ejecutar input sin validar
        public string RunSystemCommand(string userCommand)
        {
            // FALLO: el usuario podría inyectar: ; rm -rf /
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + userCommand);
            using var process = Process.Start(psi);
            process.WaitForExit();
            return "Executed";
        }

        // VULNERABILIDAD: SSRF - el backend pide URLs controladas por el usuario
        public async Task<string> FetchUrl(string userSuppliedUrl)
        {
            using var client = new HttpClient();
            // FALLO: el usuario podría pedir http://169.254.169.254/ (metadata cloud)
            var response = await client.GetStringAsync(userSuppliedUrl);
            return response;
        }

        // VULNERABILIDAD: credenciales hardcodeadas en el código
        public string GetDatabasePassword()
        {
            string password = "P@ssw0rd_Fake_123"; // VULNERABILIDAD: hardcodeada
            return password;
        }

        // VULNERABILIDAD: log de información sensible
        public void LogError(Exception ex)
        {
            // FALLO: loguea stacktrace e información sensible
            var logger = new Logger();
            logger.LogException(ex.ToString());
        }
    }

    internal class Logger
    {
        public void LogException(string message) { System.Console.WriteLine(message); }
    }
}