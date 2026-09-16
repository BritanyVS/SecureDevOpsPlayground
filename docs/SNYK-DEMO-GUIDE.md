# SNYK-DEMO-GUIDE — Guía de demos por producto Snyk

> Objetivo: demostrar el catálogo Snyk completo sobre **un solo repo**. Cada sección indica
> qué se demuestra, cómo correrlo, qué se espera detectar y cómo remediarlo/verificarlo.

## 0. Prerrequisitos

- `snyk` CLI: `npm install -g snyk` o instalador standalone; `snyk auth` con token.
  Variables: `SNYK_TOKEN`, `SNYK_ORG`.
- `snyk-api/`: Node ≥ 18, sin deps (fetch nativo).

---

## 1. Snyk Open Source

**Qué se demuestra**: vulnerabilidades en dependencias directas y transitivas + fix PR.

```bash
cd SecureDevOps.Web && npm install
snyk npm test --org=<org>           # o snyk test con path a package.json
snyk test --all-projects
# JSON para parsear:
snyk test SecureDevOps.Web/package.json --json
snyk monitor                        # registrar proyecto en la account
```

**En esta app**: `lodash` / `minimist` / `yargs-parser` son dependencias dev/demo con
vulns conocidas (fixture). Ver `security-lab/dependencies/README.md`.

**Demo en vivo**: `snyk test --all-projects` → filtrar por `High` → `snyk fix`
(vía Upgrade PR) → re-test bajo count.

---

## 2. Snyk Code (SAST estático)

**Qué se demuestra**: dataflows vulnerables en código .NET/TS, XSS, injections, etc.

```bash
snyk code test                       # escanea el repo entero
snyk code test --json
snyk code test security-lab/code     # aislar fixtures
```

**En esta app**: `security-lab/code/*.cs` son 9 escenarios diseñados para disparar el
analizador (XSS reflejado, SQLi, command i., path traversal, deserialización, SSRF,
hardcoded creds, IDOR, unsafe input). Documentación por escenario en cada `.md`.

**Demo**: escanear → abrir el issue en la consola → ver el data flow → aplicar el fix
(versión "secure" ya presente en el archivo) → re-escaneo → issue resuelto.

---

## 3. Snyk Secrets

**Qué se demuestra**: detección de secretos/gitignored leaks.

```bash
snyk secrets scan --report-security-website <carpeta>   # si el CLI lo soporta
snyk secrets scan security-lab/secrets/*.example.json   # fixtures
# Ejemplo de script en .github/workflows (solo secretos): ver docs/API.md no hay...
```

**En esta app**: `security-lab/secrets/*` y `.env.lab` contienen **valores ficticios** a
propósito (cuando un secreto de verdad se detecte → acción de rotación). Ver
`ai-agent/prompts/review-secrets.md`.

---

## 4. Snyk Container

**Qué se demuestra**: CVEs en imágenes base y en capas.

```bash
docker build -f docker/Dockerfile.backend -t securedevops-api:lab .
snyk container test securedevops-api:lab --file=docker/Dockerfile.backend
snyk container monitor securedevops-api:lab --file=docker/Dockerfile.backend
```

**En esta app**: imágenes Dockerfile.backend/frontend endurecidas. Fixtures inseguros en
`security-lab/container/` (para comparar primero enduro vs inseguro).

**Demo**: `snyk container test` sobre `Dockerfile.insecure-node` → N críticos;
luego sobre `Dockerfile.backend` → menos/menos críticos. Explica no-root, slim/alpine,
limpieza de cache.

---

## 5. Snyk IaC

**Qué se demuestra**: misconfiguraciones en Terraform/CloudFormation.

```bash
cd infrastructure/terraform
snyk iac test            # versión segura → esperamos 0-2 high/critical
cd ../../security-lab/iac
snyk iac test insecure.tf   # fixture → muchos issues
```

**En esta app**: `insecure.tf` (S3 público, SG 0.0.0.0/0, RDS sin cifrar, IAM `*`,
creds hardcodeadas) vs `main.tf` (menor privilegio, cifrado, private subnets, logs).

---

## 6. Snyk API & Web (DAST)

**Qué se demuestra**: escaneo de la aplicación en ejecución (ruta, inyecciones,
cabeceras, auth).

- Arranca la app: `docker compose up --build` (o `dotnet run` en 7196).
- Añade URL/domain en la UI de Snyk AppRisk (configura credenciales con users demo de
  `docs/DAST-SCENARIOS.md` y `SecurityLab__Enabled=true` para los endpoints lab).
- O prueba manual con el script de curl de `docs/DAST-SCENARIOS.md`.

**Ojo**: los `curl` de `docs/DAST-SCENARIOS.md` #7 solo funcionan con lab habilitado.

---

## 7. Snyk API (curl/scripts)

```bash
cd snyk-api
$env:SNYK_TOKEN="tu-token"
npm run orgs
npm run projects <orgId>
npm run issues <orgId> <projectId>
npm run report <orgId>
```

**Demo**: reporte JSON con totals → se puede parsear en el dashboard.

---

## 8. AI Agent + MCP (agéntico)

- `ai-agent/README.md`: el ciclo PLAN→…→REPORT con un agente.
- `ai-agent/MCP.md`: cómo enchufar un MCP server de Snyk al agente.
- `ai-agent/prompts/*`: prompts para feature, review, fix, deps, docker, terraform, api,
  secrets y full-scan.

---

## 9. Prio (vulnerability grouping) — opcional

Con `SNYK_TOKEN` + Dashboard, explicar cómo una vuln se agrupa con otras (same root cause)
y el score (llamado también "Priority Score"). Recomendación: activate `snyk monitor`
para tener el proyecto en la account y usar la UI para priorización.

---

## Check-list de setup del entorno

- [ ] `snyk auth` OK y `SNYK_TOKEN`/`SNYK_ORG` en el shell/CI.
- [ ] Build local verde: `dotnet test`, `npm run build`.
- [ ] Docker build de las 2 imágenes OK.
- [ ] (Opcional) credenciales demo registradas en Snyk AppRisk antes de DAST.
- [ ] `snyk-api/.env` configurado (sin commit-ear `SNYK_TOKEN`).