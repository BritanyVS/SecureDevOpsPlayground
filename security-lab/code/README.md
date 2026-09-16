# security-lab/code/ — Escenarios para Snyk Code (SAST)

> ⚠️ **ESTE DIRECTORIO ES UN LABORATORIO CONTROLADO.** Todo el código aquí es intencionalmente
> vulnerable para que Snyk Code lo detecte. **No está compilado ni referenciado por la aplicación**
> principal y **no debe copiarse a código de producción**.

## Objetivo

Demostrar cómo **Snyk Code** detecta vulnerabilidades en el código fuente, cómo interpretar el
reporte y cómo corregirlo. La mecánica de cada escenario es:

1. Un archivo contiene el código **vulnerable** (`VulnerableScenario` / método `Run*`).
2. Debajo, lado a lado, el **código corregido** (`SecureScenario` / método `Run*Secure`).
3. Un `README.md` explica: vulnerabilidad esperada, cómo la detecta Snyk, impacto y remediación.

## Inventario

| # | Escenario | Archivo | Finding esperado en Snyk Code |
|---|---|---|---|
| 01 | SQL Injection | `01-sql-injection.md` + `sql-injection.cs` | SQL Injection |
| 02 | Command Injection | `02-command-injection.md` + `command-injection.cs` | Command Injection |
| 03 | Path Traversal | `03-path-traversal.md` + `path-traversal.cs` | Path Traversal |
| 04 | Cross-Site Scripting | `04-xss.md` + `xss.cs` | Reflected/Stored XSS |
| 05 | Insecure Deserialization | `05-insecure-deserialization.md` + `insecure-deserialization.cs` | Insecure Deserialization |
| 06 | SSRF | `06-ssrf.md` + `ssrf.cs` | Server-Side Request Forgery |
| 07 | Hardcoded credentials | `07-hardcoded-credentials.md` + `hardcoded-credentials.cs` | Hardcoded secret / credential |
| 08 | Improper authorization (IDOR) | `08-improper-authorization.md` + `improper-authorization.cs` | Improper authorization / BOLA-IDOR |
| 09 | Unsafe input handling | `09-unsafe-input-handling.md` + `unsafe-input-handling.cs` | Improper input validation / ReDoS |

> Además de estos escenarios, la demo de **Snyk Code sobre la aplicación real** se hace con:
> - `LabController.cs` de la API (endpoints activables con `SecurityLab__Enabled=true`).
> - `LabVulnerabilitiesService.cs` (movido aquí desde `Services/` — ver abajo).

## Cómo ejecutar Snyk Code sobre el laboratorio

```bash
# Desde la raíz del repo
snyk code test security-lab/code

# O todo el repo (detectará también los fixtures del directorio SnykLab/)
snyk code test
```

## Archivos movidos desde la aplicación real

- `LabVulnerabilitiesService.cs` — antes vivía en `SecureDevOps.API/Services/` (código muerto,
  sin referencias de DI) y se movió aquí para mantener la aplicación principal **libre de
  vulnerabilidades intencionales**. Contiene: MD5 para hashing, secreto hardcodeado, XSS por
  concatenación, log de excepciones sensibles, SQLi y path traversal.

## Nota

Para una demo de **remediación asistida por agente AI** (Evo/Codex), usa estos archivos así:

1. `snyk code test security-lab/code` → obtener los findings.
2. Un agente corrige el `README` reproyectando el código vulnerable a la versión `*Secure`.
3. `snyk code test security-lab/code` de nuevo → verificar que los findings desaparecen.
4. Generar el reporte con `snyk code test --json > report.json`.