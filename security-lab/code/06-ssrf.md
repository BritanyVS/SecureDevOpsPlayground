# 06 — SSRF (Server-Side Request Forgery)

## Código vulnerable
Ver `ssrf.cs` (método `FetchUrl`).

```csharp
using var client = new HttpClient();
return await client.GetStringAsync(url);   // url controlada por el atacante
```

## Vulnerabilidad esperada
- **Snyk Code**: `Server-side Request Forgery (SSRF)` (Severity: High).
- Snyk rastrea `url` desde el controlador HTTP hacia `HttpClient.GetStringAsync`.

## Impacto
- Acceso al **metadata service de cloud** (`169.254.169.254` → credenciales IAM).
- Escaneo de hosts internos, redis/memcached, admin endpoints en `127.0.0.1`.
- File reading vía `file://` en algunos stacks.

## Remediación
1. Validar esquemas permitidos (https/http).
2. Bloquear IPs privadas y link-local (10/8, 172.16/12, 192.168/16, 127.0.0.0/8, 169.254/16).
3. Preferir **allowlists de dominio**; re-resolver DNS y comprobar la IP real.
4. Usar un proxy/egress dedicado y sin acceso a la red interna.

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`FetchUrlSecure`), Snyk no reporta SSRF.
```