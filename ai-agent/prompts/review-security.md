# Prompt: Revisión de seguridad del código (Snyk Code)

```text
Realiza una revisión de seguridad del código SIGUIENDO el ciclo agentic.

PASO 1 — RECOPILA
- `snyk code test` (y `snyk code test --report` si hay Snyk org configurada).
- `snyk test --all-projects`
- `snyk iac test infrastructure/terraform`
- `snyk container test <img>`
- `snyk secrets scan <carpetas>`

PASO 2 — ANALIZA cada finding:
  - Clase/regla (CWE/OWASP) y severity.
  - Dataflow que Snyk reporta (fuente → sink).
  - Impacto real en este repo (¿exploitable?).

PASO 3 — CLASIFICA: crítico/alto/medio/bajo + falsos positivos y por qué.

PASO 4 — DEJA SIN CAMBIAR el código; entrega un reporte markdown:
  - Tabla: severity | regla/CWE | archivo:línea | descripción | remediación
  - Arréglalo después solo si se te pide expresamente.

Salida mínima: 3 tablas (Code / Open Source / Otros) y una recomendación priorizada.
```