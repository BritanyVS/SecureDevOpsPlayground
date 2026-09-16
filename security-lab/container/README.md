# security-lab/container/ — Snyk Container

> Ejemplos **controlados** de malas prácticas Docker/container para que **Snyk Container**
> los detecte. La imagen de producción (raíz `Dockerfile` y `docker/Dockerfile.backend`) es
> la versión SEGURA. **No uses estos fixtures en producción.**

## Archivos

| Archivo | Malas prácticas | Qué reporta Snyk Container |
|---|---|---|
| `Dockerfile.insecure-dotnet` | .NET 5 EOL, `USER root`, secrets en `ENV`, sin healthcheck, imagen sin versionar inline | Deployed image tagged as EOL, running as root, secret in env, unfixed base image |
| `Dockerfile.insecure-node` | Node 16 EOL, `USER root`, secrets en `ENV`, copy de todo el repo, sin HEALTHCHECK | EOL base image, root user, package vulns, OS vulns |
| `package.json` (fixture) | Dependencias npm obsoletas dentro del container | Vulnerabilities in the container registry |

## Cómo ejecutar el análisis

```bash
# Build de un fixture inseguro (fragmentos básicos)
docker build -f security-lab/container/Dockerfile.insecure-node -t lab-insecure-node:demo .
docker build -f security-lab/container/Dockerfile.insecure-dotnet -t lab-insecure-dotnet:demo .

# Análisis con Snyk Container
snyk container test lab-insecure-node:demo --file=security-lab/container/Dockerfile.insecure-node
snyk container test lab-insecure-dotnet:demo --file=security-lab/container/Dockerfile.insecure-dotnet
```

> Si Docker da crea las imágenes está bien; para el demo incluso basta analizar el Dockerfile
> con `--file` (Snyk completa la información de la imagen base con los manifests).

## Qué debe detectar (guiía rápida)

1. **Base image with known vulnerability / EOL**:
   `mcr.microsoft.com/dotnet/sdk:5.0` y `node:16` ya no reciben parches (EOL).
2. **Running as root**: `USER root` explícito o ausencia de `USER app`.
3. **High-severity OS packages**: paquetes apt/apk con CVEs en la imagen base.
4. **Secrets in image**: `ENV` con passwords / API keys.
5. **Unpinned / latest tag**: `FROM node:latest` sin digest.

## Remediación (la versión segura en el repo)

| Problema | Fix (ver `docker/Dockerfile.backend`, `docker/Dockerfile.frontend`) |
|---|---|
| EOL image | Actualizar a `dotnet:8.0` / `nginx-unprivileged:1.27-alpine` **con pin por digest** |
| root user | `USER app` / `nginxinc/nginx-unprivileged` (no-root por defecto) |
| secrets | Todo por variables de entorno (`JWT_SECRET`, etc.). Nunca literales |
| sin healthcheck | `HEALTHCHECK` con `/health` (backend) y `/healthz` (nginx) |
| exceso de capas/paquetes | Multi-stage; instalar solo lo necesario (`curl`) y limpiar apt cache |
| archivos sensibles en la imagen | `.dockerignore` excluye `.env`, `security-lab/`, `SnykLab/`, `*.db` |

## Cómo verificar la remediación

```bash
docker build -t securedevops-api:lab .
snyk container test securedevops-api:lab --file=Dockerfile
# Resultado esperado: sin vulnerabilidades conocidas críticas/altas de base, runner no-root.
```