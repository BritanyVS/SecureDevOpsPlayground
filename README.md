# SecureDevOpsPlayground — Snyk Security & Agentic Development Lab

> Laboratorio profesional de seguridad y desarrollo agéntico sobre una app Task Manager
> funcional: **backend seguro** (.NET 8) + **frontend** (React 19/Vite/TypeScript) +
> **escenarios vulnerables controlados y aislados** en `security-lab/` para demostrar el
> catálogo de Snyk (Open Source, Code, Secrets, Container, IaC, API & Web/DAST, API, MCP).

## Quick start

```bash
# Backend (dev, https://localhost:7196)
dotnet run --urls "https://localhost:7196" --project SecureDevOps.API/SecureDevOps.API.csproj

# Frontend (dev, http://localhost:3000; proxy /api al backend)
cd SecureDevOps.Web && npm install && npm run dev

# Docker completo (http://localhost:8080, frontend http://localhost:3000)
docker compose up --build
```

Usuarios demo (seed): `juan@gmail.com` / `contra1234`, `prueba@gmail.com` / `contra1234`,
`admin@gmail.com` / `Admin123!`.

### Variables de entorno (ver `.env.example`)

| Variable | Uso |
|---|---|
| `JWT_SECRET` | Firma de tokens JWT (tomada con precedencia sobre appsettings) |
| `SECURITYLAB__ENABLED` | Habilitar endpoints de laboratorio (`/api/lab/*`) para demos DAST |
| `ConnectionStrings__DefaultConnection` | Cadena de SQLite/BD |
| `VITE_API_URL` | URL base del frontend (`/api` en docker) |
| `SNYK_TOKEN`, `SNYK_ORG` | Scripts de `snyk-api/` y CI |

## Tests

```bash
# Backend (xUnit, 13 tests)
dotnet test tests/SecureDevOps.API.Tests/SecureDevOps.API.Tests.csproj

# Frontend
cd SecureDevOps.Web && npm run build && npm run lint
```

## Estructura del laboratorio

| Carpeta | Contenido |
|---|---|
| `security-lab/code/` | 9 escenarios Snyk Code (vuln + secure) |
| `security-lab/dependencies/` | Doc de dependencias para Snyk Open Source |
| `security-lab/secrets/` | Fixtures de secretos para Snyk Secrets (ficticios) |
| `security-lab/container/` | Dockerfiles inseguros de referencia |
| `security-lab/iac/` | Terraform inseguro (Snyk IaC) — comparar con la versión segura |
| `infrastructure/terraform/` | IaC SEGURO de referencia (VPC/RDS/ECS) |
| `docker/` | Dockerfiles endurecidos + nginx + compose |
| `snyk-api/` | Scripts Node para la Snyk REST/V1 API |
| `ai-agent/` | Workflow agentic PLAN→…→REPORT, prompts reutilizables, MCP |
| `docs/` | `API.md`, `DAST-SCENARIOS.md`, `SECURITY-ARCHITECTURE.md`, `SNYK-DEMO-GUIDE.md` |
| `tests/` | Proyecto xUnit de la API |

## Estado de seguridad (resumen)

- ✔ **IDOR corregido**: los recursos se resuelven contra el usuario del JWT
  (`ClaimTypes.NameIdentifier`), nunca contra input del cliente.
- ✔ **JWT** secreto desde `JWT_SECRET` (precedencia) → throw si falta.
- ✔ **Lab endpoint** desactivado por defecto (`SecurityLab__Enabled=false`).
- ✔ **/health**, logout, CORS y seed seguros.
- ⚠ Baseline y hallazgos previos documentados en `SECURITY-LAB-BASELINE.md`.

## Escaneos con Snyk

```bash
snyk test --all-projects          # Open Source
snyk code test                    # Code
snyk iac test infrastructure/terraform   # IaC (seguro)
snyk iac test security-lab/iac    # IaC (fixtures)
snyk container test securedevops-api:lab --file=docker/Dockerfile.backend
snyk secrets scan security-lab/secrets   # Secrets
```

Guía completa de demos: `docs/SNYK-DEMO-GUIDE.md`.

## CI/CD

`.github/workflows/ci.yml`: build + tests (push de main y PRs) y escaneos Snyk
(Open Source, Code con SARIF, IaC, Container), usando `secrets.SNYK_TOKEN`.
Existe además un pipeline heredado `Jenkinsfile` con `SnykLab/`.

## Notas

- Los fixtures inseguros (`security-lab/`, `SnykLab/`, `snyk-complete-demo/`) NO deben
  desplegarse en producción; son material didáctico.
- No se incluyen secretos reales; todos los valores de ejemplo son ficticios.