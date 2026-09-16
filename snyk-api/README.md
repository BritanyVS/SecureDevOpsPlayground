# snyk-api/ — Scripts de integración con la Snyk API

Base para consultar **organizaciones, proyectos y vulnerabilidades** desde la **Snyk API**
(REST + V1) y automatizar reportes. **No hardcodea tokens**: usa la variable `SNYK_TOKEN`.

## Requisitos

- Node.js ≥ 18 (usa `fetch` global, sin dependencias).
- Un token de **Snyk API** con permiso de lectura sobre las orgs que quieras consultar
  (Snyk → Account → API Token; alternativamente un token de Service Account).

## Configuración

```bash
# Windows PowerShell
$env:SNYK_TOKEN="tu-token-aqui"
# o Linux/macOS
export SNYK_TOKEN="tu-token-aqui"

# (Opción) org por defecto
$env:SNYK_ORG="tus-org-id"          # se obtiene con `npm run orgs`
```

Copia `.env.example` a `.env` si quieres guardar la config útil (SNYK_TOKEN nunca se
commit-ea — está en `.gitignore`).

## Uso

```bash
npm run orgs                    # lista organizaciones → guarda .orgs.json
npm run projects <orgId>        # lista proyectos de una org → .projects.json
npm run issues <orgId> <projectId>   # issues de un proyecto (V1 aggregated-issues)
npm run report <orgId> [projectId]   # reporte consolidado JSON (snyk-report.json)
```

## Endpoints usados

| Script | Endpoint | API |
|---|---|---|
| `orgs.mjs` | `GET /rest/orgs?version=<REST_VERSION>` | REST |
| `projects.mjs` | `GET /rest/orgs/{orgId}/projects?version=<REST_VERSION>` | REST |
| `issues.mjs` | `GET /v1/org/{orgId}/aggregated-issues?project_id=...` | V1 |
| `report.mjs` | REST projects + V1 aggregated-issues (por proyecto) | REST + V1 |

Variables extra (opcionales): `SNYK_API_BASE`, `SNYK_REST_VERSION`.

## Automatización

- En CI: proteger con secret `SNYK_TOKEN` (ver `.github/workflows/ci.yml`).
- Para reportes programáticos: `npm run report <orgId> > snyk-report.md` / JSON.

## Ejemplo de salida de `issues.mjs`

```text
Issues del proyecto <uuid>: 15
  [HIGH] Prototype Pollution   (lodash @ 4.17.15/4.17.21)
  [MEDIUM] Directory Traversal (tar @ ...)
Resumen por severidad: {"high":4,"medium":8,"low":3}
```

## Notas

- `snyk-report.json` y los `.orgs.json`/`.projects.json` son cache local; añádelos a
  `.gitignore` si no los quieres versionar.
- El token de Snyk da acceso según los **roles** del usuario; usa Service Accounts con
  alcance mínimo para automatización.
- Snyk Sensor/Licenses: si consultas issues de productos (Code/Os/Container/IaC), el
  reporte se alimentará con todos los tipos que la org tenga.