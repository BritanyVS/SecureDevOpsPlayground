# 🧪 Laboratorio de Seguridad con Snyk

Proyecto con **vulnerabilidades intencionales** para probar las distintas herramientas de Snyk.

> ⚠️ **IMPORTANTE**: Este proyecto es SOLO para laboratorio/educación. Nunca uses estas prácticas en código real.

## Herramientas de Snyk a probar

| Herramienta | Qué analiza | Archivos donde están las "vulnerabilidades" |
|---|---|---|
| **Snyk Code** | Código fuente (SAST) | `SecureDevOps.API/Services/LabVulnerabilitiesService.cs`, `SnykLab/SnykLabCode/*.cs` |
| **Snyk Open Source** | Dependencias | `SecureDevOps.Web/package.json`, `SecureDevOps.API/*.csproj` |
| **Snyk Secrets** | Secretos en código | `SecureDevOps.API/.env.lab`, `SnykLab/secrets/*.txt` |
| **Snyk IaC** | Infraestructura como código | `SnykLab/terraform/*.tf` |
| **Snyk Container** | Imágenes Docker | `SnykLab/containers/*.dockerfile`, `Dockerfile` |
| **Snyk API & Web** | Monitoreo / API | `Jenkinsfile` (pipeline con `snyk monitor`) |

---

## Archivos vulnerables (inventario)

### 📁 `SnykLab/SnykLabCode/` — Snyk Code
- **`InjectionExample.cs`** — SQL Injection por concatenación
- **`CryptographyWeak.cs`** — MD5, SHA1, claves hardcodeadas, semilla predecible
- **`PathTraversal.cs`** — Path Traversal al leer archivos del usuario
- **`MiscVulnerabilities.cs`** — Command Injection, SSRF, credenciales hardcodeadas, log sensible

### 📁 `SnykLab/secrets/` — Snyk Secrets
- **`api-keys.txt`** — claves "ficticias" de AWS/Azure/Stripe/GitHub/Slack + llave RSA
- **`npm-token.txt`** — token npm falso

### 📁 `SnykLab/terraform/` — Snyk IaC
- **`main.tf`** — S3 público, SG abiertos (0.0.0.0/0), RDS sin cifrado, passwords hardcodeadas, IAM con permisos `*`
- **`kubernetes.tf`** — pod como root, imagen antigua, secretos en env

### 📁 `SnykLab/containers/` — Snyk Container
- **`Dockerfile.insecure-node`** — node 16 EOL, secrets en ENV, corre como root, sin HEALTHCHECK
- **`Dockerfile.insecure-dotnet`** — .NET 5 EOL, connection string con password, root

### 🎯 En el proyecto real — Snyk Code + Open Source
- **`SecureDevOps.API/Services/LabVulnerabilitiesService.cs`** — MD5, SQLi, XSS, path traversal, secreto hardcodeado
- **`SecureDevOps.API/Controllers/LabController.cs`** — endpoints HTTP vulnerables para Snyk API & Web (DAST):
  - `GET /api/lab/xss?input=...` — Reflected XSS (devuelve `text/html` sin escapar)
  - `GET /api/lab/search?q=...` — SQL Injection (concatena el input en SQL SQLite)
  - `GET /api/lab/file?name=...` — Path Traversal (lee archivos con `Path.Combine` sin validar)
  - `GET /api/lab/ping?host=...` — Command Injection (ejecuta `/bin/sh` con el host)
  - `GET /api/lab/fetch?url=...` — SSRF (el servidor pide la URL sin validar)
  - `GET /api/lab/secret` — secrets hardcodeados expuestos en una respuesta
- **`SecureDevOps.API/uploads/lab-secret.txt`** — archivo demo para el Path Traversal
- **`SecureDevOps.API/.env.lab`** — secretos ficticios en "variables de entorno" (Snyk Secrets)
- **`SecureDevOps.API/*.csproj`** — `log4net 2.0.12` (CVE-2018-1285) y `Newtonsoft.Json 13.0.1` (CVE-2024-21905) — Snyk Open Source
- **`SecureDevOps.Web/package.json`** — `lodash 4.17.15`, `minimist 1.2.0`, `yargs-parser 5.0.1` (5 vulns: 4 high + 1 critical) — Snyk Open Source
- **`Jenkinsfile`** — pipeline con los 6 escaneos Snyk

---

## Cómo correr cada prueba

### 1. Snyk Code
```bash
snyk code test
snyk code test SecureDevOps.API
```

### 2. Snyk Open Source
```bash
cd SecureDevOps.API && snyk test --all-projects
cd SecureDevOps.Web && snyk test
```

### 3. Snyk Secrets
```bash
snyk code test   # y revisar la sección de secrets en la salida
```

### 4. Snyk IaC
```bash
cd SnykLab/terraform && snyk iac test main.tf
```

### 5. Snyk Container
```bash
docker build -t securedevops-api:lab .
snyk container test securedevops-api:lab --file=Dockerfile
snyk container test --file=SnykLab/containers/Dockerfile.insecure-dotnet
```

### 6. Snyk API & Web
```bash
# Sube los artifacts a la consola de Snyk para monitoreo continuo
snyk monitor --all-projects
snyk monitor --project-name=securedevops-web

# Probar API de Snyk
curl --request GET --url 'https://api.snyk.io/rest/orgs?version=2024-01-04' \
     --header "Authorization: token $SNYK_TOKEN"
```

---

## Notas
- Todos los secretos son **100% ficticios**.
- El `Jenkinsfile` usa el comando `snyk test` con `--severity-threshold=high`, así que **fallará el pipeline si hay vulns altas** (comportamiento esperado en el lab).
- Los archivos `.cs` de `SnykLab` NO se compilan en el proyecto (namespace separado) — solo sirven para que Snyk Code los escanee.
- `Newtonsoft.Json` se degradó de 13.0.3 → 13.0.1 a propósito (CVE-2024-21905) y `log4net` se agregó como dependencia directa.

## Sanitizer recomendado (código REAL, no en el lab)

> ⚠️ **NO agregues sanitizers a `LabController.cs` ni a los archivos de `SnykLab/`** — eso ocultaría los hallazgos que el escáner debe detectar. La tabla siguiente es para cuando corrijas vulnerabilidades en código de producción.

| Vulnerabilidad | Sanitizer recomendado (.NET) |
|---|---|
| **XSS** (output HTML) | `System.Net.WebUtility.HtmlEncode(input)` o `System.Security.SecurityElement.Escape(input)` antes de insertar en HTML |
| **SQL Injection** | Parametrizar siempre: `command.Parameters.AddWithValue("@q", q)`; nunca concatenar strings SQL |
| **Path Traversal** | `var full = Path.GetFullPath(Path.Combine(baseDir, name)); if (!full.StartsWith(baseDir)) return BadRequest();` |
| **Command Injection** | `ProcessStartInfo` con `ArgumentList` (nunca `UseShellExecute`) o validar `host` contra una whitelist/expresión regular |
| **SSRF** | Validar que `url` sea `https://` y que el host resuelva a una IP pública permitida; usar `HttpClient` con `AllowedHosts`; bloquear rangos de link-local (169.254/16, 10/8, 127.0.0.1 u 128.0/16) |
| **Secrets hardcodeados** | Mover a variables de entorno / Key Vault / Snyk secrets; nunca escribir valores literales en código |