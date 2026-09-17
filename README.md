# SecureDevOpsPlayground — Laboratorio Snyk

> Un solo proyecto sencillo, **vulnerable a propósito** y con vulnerabilidades
> **controladas y fáciles de corregir**, para demostrar los productos Snyk:
> **Code, Open Source, Secrets, IaC y Container**, además de endpoints para
> **Snyk API & Web (DAST)**.

Task Manager funcional con **backend .NET 8** (`SecureDevOps.API`) + **frontend React/Vite**
(`SecureDevOps.Web`) + **SQLite**.

## Quick start

```bash
# Desarrollo
dotnet run --urls "https://localhost:7196" --project SecureDevOps.API/SecureDevOps.API.csproj
cd SecureDevOps.Web && npm install && npm run dev   # http://localhost:3000 (proxy /api)

# Docker completo (frontend http://localhost:3000 · API http://localhost:8081)
docker compose up --build
```

Usuarios demo (seed): `juan@gmail.com` / `contra1234` · `admin@gmail.com` / `Admin123!`

## Variables de entorno (ver `.env.example`)

| Variable | Uso |
|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena SQLite (por defecto local o `/app/data/SecureDevOpsDb.db` en Docker) |
| `VITE_API_URL` | URL base del frontend (`/api` en Docker) |

## Escaneos con Snyk

```bash
snyk auth            # si no has iniciado sesión
snyk code test                    # Snyk Code (SAST, backend)
snyk test --all-projects          # Snyk Open Source (frontend)
snyk secrets scan .               # Snyk Secrets (secrets.example.json)
snyk iac test iac/main.tf         # Snyk IaC
snyk container test snyk-lab-backend --file=Dockerfile   # Snyk Container (imagen local)
```

Resultados esperados y resolución de cada hallazgo: **`docs/VULNERABILITIES.md`** (y el PDF del mismo nombre).

## Estructura

```
SecureDevOps.API/        Backend .NET 8 (vulnerable por diseño, endpoints /api/lab/* para DAST)
SecureDevOps.Web/        Frontend React/Vite con dependencias antiguas (Snyk Open Source)
docs/                    VULNERABILITIES.md + PDF  ← inventario exacto de lo implementado
iac/                     Terraform inseguro (Snyk IaC)
secrets.example.json     Fixture de secretos (Snyk Secrets) — ficticios
Dockerfile               Backend (imagen sdk + root → Snyk Container)
docker-compose.yml       Backend + frontend con volumen SQLite
```

## Notas de seguridad

- Todo es **material didáctico**: la app y la infra incluidas son intencionalmente inseguras.
  No desplegar en producción tal cual. Arreglos de 1–3 líneas descritos en `docs/VULNERABILITIES.md`.
- No hay secretos reales; todos los valores de ejemplo son ficticios.