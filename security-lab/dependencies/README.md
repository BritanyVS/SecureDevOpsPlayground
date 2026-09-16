# security-lab/dependencies/ — Snyk Open Source

> Documenta el ecosistema de dependencias del proyecto y cómo **Snyk Open Source** lo analiza.
> El objetivo es poder demostrar detección de vulnerabilidades en dependencias **directas y
> transitivas**, interpretar severidades y remediarlas.

## Componentes con dependencias

| Módulo | Manifest | Plataforma |
|---|---|---|
| Backend API | `SecureDevOps.API/SecureDevOps.API.csproj` | .NET (NuGet) |
| Frontend SPA | `SecureDevOps.Web/package.json` + `package-lock.json` | npm (Node.js) |
| Scripts Snyk API | `snyk-api/package.json` | npm (Node.js) |
| Tests | `tests/SecureDevOps.API.Tests/SecureDevOps.API.Tests.csproj` | .NET (NuGet) |

## Dependencias directas (manifest)

### Backend (`SecureDevOps.API.csproj`)
| Paquete | Versión | Rol | Crítica |
|---|---|---|---|
| `BCrypt.Net-Next` | 4.2.0 | Hash de contraseñas | **Alta** (autenticación) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.20 | JWT | **Alta** |
| `Microsoft.EntityFrameworkCore.*` | 8.0.20 | ORM / BD | Alta |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger | Media |
| `Newtonsoft.Json` | 13.0.1 | JSON | **Media (histórico CVE-2024-21905)** |
| `log4net` | 3.3.0 | Logging | Baja (declarada para demo) |

### Frontend (`package.json`)
| Paquete | Versión | Rol | Nota Snyk |
|---|---|---|---|
| `axios` | ^1.18.1 | HTTP | Transmitida |
| `lodash` | **^4.17.15** | Utils | **Demo: potencia findings históricos** |
| `react` / `react-dom` | ^19.2.7 | UI | Transmitida |
| `react-router-dom` | ^7.18.1 | Routing | Transmitida |
| `minimist` (dev) | **^1.2.0** | CLI | **Demo: hallazgos conocidos** |
| `yargs-parser` (dev) | **^5.0.1** | CLI | **Demo: hallazgos conocidos** |
| `vite` / `oxlint` / `typescript` | — | Build/lint | Mantener al día |

## Dependencias transitivas (mirada de Snyk)

Snyk Open Source resuelve el **árbol completo** (`package-lock.json`, `project.assets.json` de
NuGet) y compara cada paquete contra su base de conocimientos (VulnDB / Snyk Database).
Ejemplo de transitivas que suelen aparecer en la demo del frontend: `lodash.template`,
`tar`, `chokidar`, etc., bajo `vite`. **La demo profesional**: mostrar que un paquete que NO
declaramos directamente (transitiva) puede traer CVEs gracias a una dependencia top-level.

```bash
# Ver el árbol de dependencias (para entender qué es transitivo)
cd SecureDevOps.Web && npm ls --all
cd SecureDevOps.API && dotnet list package --include-transitive
```

## Cómo analizarlo con Snyk Open Source

```bash
# 1) Test local (no requiere app ID)
cd SecureDevOps.API && snyk test --all-projects
cd SecureDevOps.Web && snyk test

# 2) Monitoreo continuo (sube el snapshot a la consola Snyk)
snyk monitor --all-projects --org=TU-ORG

# 3) JSON para automatización / reportes
cd SecureDevOps.Web && snyk test --json > snyk-os.json
```

## Qué debe detectar Snyk en la demo

- **Directas**: si `Newtonsoft.Json 13.0.1` está fijada abajo, reporta CVE-2024-21905.
- **Frontend**: `lodash`, `minimist`, `yargs-parser` (vulnerabilidades conocidas de medias a
  críticas, incluyendo Prototype Pollution).
- **Transitivas**: cualquier CVE heredado de `vite` o del árbol npm.

## Remediación (mensaje de la demo)

1. Usar el **fix recomendado por Snyk** (`snyk fix` / bot **Snyk Fix PR** en GitHub).
2. `bump` de versiones para quitar CVEs (`npm audit fix`, `dotnet update package`).
3. Establecer política: `--severity-threshold=high` como **guard en CI** (ver
  `.github/workflows/ci.yml` y `Jenkinsfile`).

> ⚠️ Nota: para una demo de Open Source con hallazgos, las versiones marcadas como "Demo"
> se mantienen de forma **controlada**. Para el entorno real, remedia fijando las versiones
> seguras recomendadas por Snyk (`snyk test --show-vulnerable-paths`).