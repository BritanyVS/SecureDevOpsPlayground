# ai-agent/ — Desarrollo asistido por agentes de IA (Evo by Snyk / Codex) con seguridad

Este directorio convierte al repositorio en un **workflow de agentic development security**:
un agente de IA (Codex, Evo, copilotos) modifica el código con un ciclo de seguridad
**PLAN → IMPLEMENT → TEST → SECURITY SCAN → ANALYZE → REMEDIATE → TEST AGAIN →
SECURITY SCAN AGAIN → REPORT**.

La filosofía: **el guardrail de seguridad es un loop, no un evento**. Todo cambio de código
pasa por un escaneo Snyk antes y después.

## Ciclo de trabajo (PLAN → … → REPORT)

```text
        ┌────────────────────────────────────────────────────────────┐
        ▼                                                            │
   1. PLAN ──► 2. IMPLEMENT ──► 3. TEST ──► 4. SECURITY SCAN ──► 5. ANALYZE ──┐
   (leer repo,                                                           │   │
    definir alcance)   ▲                                                │   ▼
                       └──────────────── 9. REPORT ◄── 8. SCAN AGAIN ◄─ 7. TEST AGAIN ◄─ 6. REMEDIATE ─┘
```

| Paso | Qué hace el agente | Herramienta |
|---|---|---|
| 1. PLAN | Lee `SECURITY-LAB-BASELINE.md`, `docs/SECURITY-ARCHITECTURE.md`, estructura del repo; define alcance sin tocar código | lectura / `tree` |
| 2. IMPLEMENT | Aplica el cambio (feature, fix, endpoint, test, Dockerfile, Terraform) | edit/crear archivos |
| 3. TEST | Ejecuta tests y build | `dotnet test`, `npm run build` |
| 4. SECURITY SCAN | Escanea el estado NUEVO con Snyk | `snyk test`, `snyk test --all-projects`, `snyk code test`, `snyk iac test`, `snyk container test` |
| 5. ANALYZE FINDINGS | Interpreta los findings (severidad, dataflow, regla) | Snyk CLI/API/MCP o Web UI |
| 6. REMEDIATE | Corrige el código según las guías de `security-lab/` | edit |
| 7. TEST AGAIN | Re-ejecuta tests | `dotnet test`, `npm run build` |
| 8. SECURITY SCAN AGAIN | Re-escanea para verificar que el finding desapareció | Snyk CLI/API |
| 9. REPORT | Resumen: qué se cambió, findings antes/después, cómo verificar | este fichero, `FINAL-IMPLEMENTATION-REPORT.md` |

### Criterios de "done"

1. Tests verdes (`dotnet test`, `npm run build`).
2. `snyk ... test` sin regresiones respecto a la línea base, o **0 críticos/altos**.
3. Cambios ≥ de una decisión de seguridad explicada en PR/commit (qué y por qué).

## Prompts reutilizables (en `prompts/`)

| Archivo | Uso |
|---|---|
| `create-feature.md` | Pedir a un agente crear una funcionalidad nueva segura |
| `review-security.md` | Revisión de seguridad estática del código actual |
| `fix-vulnerabilities.md` | Remediar findings de Snyk |
| `review-dependencies.md` | Revisar dependencias / Snyk Open Source |
| `review-docker.md` | Revisar Dockerfiles y Snyk Container |
| `review-terraform.md` | Revisar IaC y Snyk IaC |
| `review-api.md` | Revisar endpoints/API y autorización (DAST friendly) |
| `review-secrets.md` | Auditar secretos en el repo |
| `run-security-scan.md` | Comandos exactos para correr todos los escaneos |

## Ejecución de escaneos (resumen)

```bash
snyk test --all-projects                        # Snyk Open Source
snyk code test                                  # Snyk Code
snyk secrets scan security-lab/secrets          # Snyk Secrets (según CLI)
snyk iac test infrastructure/terraform          # Snyk IaC (versión segura)
snyk iac test security-lab/iac                  # Snyk IaC (fixtures inseguros)
snyk container test securedevops-api:lab --file=Dockerfile   # Snyk Container
# DAST: ver docs/DAST-SCENARIOS.md
```

## Reglas para el agente (prompt base)

1. No modificar la línea base sin explicar el porqué.
2. No introducir vulnerabilidades en `main`; los ejemplos viven en `security-lab/`.
3. Ejecutar tests tras cada cambio.
4. Nunca escribir secretos reales; leerlos de variables de entorno.
5. Documentar cada hallazgo en `security-lab/*` y `docs/*`.