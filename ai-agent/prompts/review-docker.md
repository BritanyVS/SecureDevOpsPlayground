# Prompt: Revisar Dockerfiles / Snyk Container

```text
Revisa la seguridad de los contenedores de este repo.

IMAGENES/archivos
- docker/Dockerfile.backend
- docker/Dockerfile.frontend
- Dockerfile (raíz)
- security-lab/container/Dockerfile.insecure-* (fixtures, NO corregir, documentar)

PASOS
1. Analiza el Dockerfile: base image, user, multi-stage, copies, apt/pip/npm install,
   HEALTHCHECK, metadata (labels, USER, EXPOSE).
2. `snyk container test <imagen> --file=<Dockerfile> --exclude-app-vulns` (si tienes imagen local)
   o revisa el scan de Container que haga el CI.
3. Reporta por hallazgo: imagen base, capa, CVE, severity, camino de paquete, fix.

CHECKLIST DE BUENAS PRÁCTICAS
- Base image pinned y oficial; preferir variantes `-slim`/`-alpine` o no-root.
- `COPY --chown` para copias; `USER` no-root (backend ya usa `app`).
- Instalar dependencias vía lockfiles (`npm ci`), `apt-get update && apt-get install -y --no-install-recommends`.
- Limpiar cachés (`rm -rf /var/lib/apt/lists/*`, `npm cache clean --force`).
- HEALTHCHECK presente.
- Minimizar paquetes innecesarios y secrets en `ARG`/`ENV`.

ENTREGABLE: tabla antes/después aplicando las mejoras NO destructivas.
No revises los `Dockerfile.insecure-*` como código a corregir; son demo.
```