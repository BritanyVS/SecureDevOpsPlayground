# MCP — Model Context Protocol para el agente de seguridad

> **¿Qué es MCP?** *Model Context Protocol* es un estándar abierto (Anthropic) que permite a
> un asistente de IA (Codex, Claude, Evo, etc.) conectarse a herramientas externas mediante
> un protocolo tipo JSON-RPC, exponiendo **herramientas** (tools) como si fueran funciones
> locales del modelo, junto con **recursos** (resources) de contexto.

Para el flujo de "ai-agent security" de este laboratorio, el pipeline se vería así:

```text
  Agent (Codex/Evo/Claude)
        │  tools incluidas vía MCP: run_command, read_file, edit_file...
        ▼
  MCP SERVER (configurado en ~/.cursor/mcp.json , codex mcp.json, opencode.json...)
        │  → snyk-mcp (conexión a la API de Snyk)
        ▼
  Snyk API (orgs, projects, issues, test, severity, fix suggestion)
```

**Roles en el ciclo:**

| Componente | Rol |
|---|---|
| Agent de IA | Orquesta el ciclo PLAN→…→REPORT; llama a las tools del MCP |
| Snyk MCP server | Expone herramientas: `snyk test`, `snyk list-issues`, `get-remediation`, `scan-project`, `results report`… |
| Snyk API | Ejecuta operaciones reales (escaneo, consulta, fix hints) usando token de API |
| App (este repo) | Código analizado/remediado; fixtures en `security-lab/` |

## Ejemplo de configuración MCP (Codex / opencode / Cursor, adaptar)

```json
{
  "mcpServers": {
    "snyk": {
      "command": "npx",
      "args": ["-y", "@snyk/mcp-server"],
      "env": { "SNYK_TOKEN": "${SNYK_TOKEN}", "SNYK_ORG_ID": "${SNYK_ORG}" }
    }
  }
}
```

- El token se lee de `SNYK_TOKEN` (nunca hardcodeado).
- El prompt base del agente arranca con:

```text
Usa la herramienta snyk del MCP para escanear el proyecto y acceder a los findings.
Nunca inventes resultados: genera los outputs con las tools del server snyk y pásalos
tal cual a tu reporte.
```

## Herramientas típicas del servidor Snyk-MCP (según disponibilidad)

| Tool | Uso en el ciclo |
|---|---|
| `scan` / `test` | Escanciar el proyecto actual (Open Source, Code, Container, IaC) |
| `get_issues` / `list_issues` | Traer findings de un proyecto/org |
| `get_org / get_orgs` | Contexto organizativo |
| `analyze` / `remediate` | Sugerencia de fix para un issue |
| `report` / `summary` | Consolidar resultados |

## Flujo MCP en este repo

1. `npm run build` y `dotnet test` (pre-condición vía `run_command`).
2. `snyk_scan` → findings JSON.
3. `get_issues` filtra por severity; el agente decide qué arreglar.
4. El agente edita código (edit tools).
5. Re-escaneo (`snyk test` de nuevo) → verificar decremento.
6. `report` con antes/después.

## Sustituir MCP por CLI

No siempre es necesario MCP: el CLI de Snyk (`snyk test --json`, `snyk iac test --json`)
produce el mismo JSON que el agente puede parsear. MCP es preferible cuando:

- Se quiere que el agente consulte la **lista de issues persistida** en la Snyk account.
- Se quiere feedback continuo (no solo escaneo local).
- El agente usa modelos con tool-calling nativo.

## Nota de honestidad

Este repositorio documenta el MCP y su configuración. Alijo en `docs/SNYK-DEMO-GUIDE.md`
detallas exactas de cómo correr cada escaneo con el CLI; usa MCP solo si tu entorno tiene
el servidor de Snyk disponible (y documenta la versión del server que uses).
**No inventes** características específicas de un servidor MCP que no hayas verificado.