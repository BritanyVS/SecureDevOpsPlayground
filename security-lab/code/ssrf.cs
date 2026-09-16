using System.Net.Sockets;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: SSRF (SERVER-SIDE REQUEST FORGERY)
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class SsrfLab
{
    // VULNERABLE: el servidor hace peticiones HTTP a URLs controladas por el usuario.
    // GET /fetch?url=http://169.254.169.254/latest/meta-data → metadata de cloud.
    public async Task<string> FetchUrl(string url)
    {
        using var client = new HttpClient();
        // FALLO: no se valida el esquema ni el host → SSRF contra 127.0.0.1, metadata, etc.
        return await client.GetStringAsync(url);
    }

    // ✅ CORREGIDO: validar esquema + resolución DNS a IP pública permitida.
    public async Task<string> FetchUrlSecure(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException("Invalid URL scheme.");
        }

        var host = uri.DnsSafeHost;
        var addresses = await Dns.GetHostAddressesAsync(host);
        foreach (var address in addresses)
        {
            // Bloquear rangos privados y link-local.
            if (IsBlockedAddress(address))
            {
                throw new UnauthorizedAccessException("Blocked address range.");
            }
        }

        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    private static bool IsBlockedAddress(System.Net.IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return address.Equals(System.Net.IPAddress.Loopback)
            || bytes[0] switch
            {
                10 or 127 => true,                          // 10.0.0.0/8, 127.0.0.0/8
                172 when bytes[1] is >= 16 and <= 31 => true, // 172.16.0.0/12
                192 when bytes[1] == 168 => true,           // 192.168.0.0/16
                169 when bytes[1] == 254 => true,           // 169.254.0.0/16 (link-local / metadata)
                _ => false
            };
    }
}