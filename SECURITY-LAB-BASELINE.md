# SECURITY-LAB-BASELINE.md — Informe de análisis del proyecto

> Fecha del análisis: 2026-09-16
> Alcance: repo `SecureDevOpsPlayground` completo (rama `main`, antes de las mejoras de esta fase).

Este documento es la **línea base (baseline)** del proyecto tal y como existe. Describe arquitectura,
tecnologías, dependencias, endpoints, flujos, modelo de datos, superficies de ataque y qué elementos
sirven para demostrar cada producto de Snyk. Los cambios posteriores a esta fecha se documentan en
`FINAL-IMPLEMENTATION-REPORT.md`.

---

## 1. Arquitectura actual

El proyecto es una **aplicación web de gestión de tareas (Task Manager) con JWT** compuesta por:

```
SecureDevOpsPlayground/
├── SecureDevOps.API/        → Backend REST (.NET 8 Web API + EF Core + SQLite)
├── SecureDevOps.Web/        → Frontend SPA (React 19 + Vite + TypeScript + React Router)
├── SnykLab/                 → Fixtures intencionalmente inseguros (Code, secrets, Docker, Terraform)
├── snyk-complete-demo/      → Otro proyecto demo (e-commerce) SIN seguimiento en git (untracked)
├── Dockerfile               → Dockerfile del backend (raíz)
├── Jenkinsfile              → Pipeline Jenkins con escaneos Snyk
└── SecureDevOpsPlayground.slnx → Solución .NET (contiene solo SecureDevOps.API)
```

- **Comunicación frontend→backend**: HTTP JSON. En desarrollo, Vite hace proxy de `/api` hacia
  `https://localhost:7196`. En producción, el frontend apunta a `VITE_API_URL`.
- **Despliegue histórico**: backend en Render, frontend en Vercel (`vercel.json` con proxy `/api`).

## 2. Tecnologías y versiones

| Capa | Tecnología | Versión |
|---|---|---|
| Backend | .NET | 8.0.424 (SDK instalado) |
| Backend | ASP.NET Core Web API + EF Core (SQLite) | 8.0.20 |
| Frontend | React + Vite + TypeScript | React 19, Vite 8, TS 6 |
| Base de datos | SQLite (archivo `SecureDevOpsDb.db`) | Local |
| Auth | JWT Bearer (HS256) + BCrypt | `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.20`, `BCrypt.Net-Next 4.2.0` |

## 3. Dependencias

### Backend (`SecureDevOps.API/SecureDevOps.API.csproj`)
Directas:
- `BCrypt.Net-Next` 4.2.0 — hash de contraseñas.
- `log4net` 3.3.0 — logging (declarada, con potencial Snyk Open Source).
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.20.
- `Microsoft.AspNetCore.OpenApi` 8.0.20.
- `Microsoft.EntityFrameworkCore.Design` 8.0.20.
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.20.
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.20.
- `Swashbuckle.AspNetCore` 6.6.2 — Swagger/OpenAPI.
- `Newtonsoft.Json` 13.0.1 — serialización JSON (degradada a propósito; CVE-2024-21905 documentada en `SnykLab/README.md`).

### Frontend (`SecureDevOps.Web/package.json`)
Directas: `axios` ^1.18.1, `lodash` ^4.17.15, `react` ^19.2.7, `react-dom` ^19.2.7, `react-router-dom` ^7.18.1.
Dev: `@vitejs/plugin-react`, `typescript`, `vite`, `oxlint`, `minimist` 1.2.0, `yargs-parser` 5.0.1.
> `lodash`, `minimist` y `yargs-parser` están fijados en versiones con hallazgos conocidos de Snyk
> Open Source (fixture de laboratorio heredado).

## 4. Endpoints actuales

| Método | Ruta | Auth | Uso |
|---|---|---|---|
| POST | `/api/auth/register` | No | Registrar usuario |
| POST | `/api/auth/login` | No | Login, devuelve JWT |
| GET | `/api/taskitem` | Sí | Listar tareas |
| GET | `/api/taskitem/{id}` | Sí | Obtener tarea por id |
| POST | `/api/taskitem` | Sí | Crear tarea |
| PUT | `/api/taskitem/{id}` | Sí | Editar tarea |
| DELETE | `/api/taskitem/{id}` | Sí | Eliminar tarea |
| GET | `/api/user/profile` | Sí | Perfil del token |
| GET | `/api/lab/xss` | No | **Laboratorio**: XSS reflejado (DAST) |
| GET | `/api/lab/search` | No | **Laboratorio**: SQL Injection |
| GET | `/api/lab/file` | No | **Laboratorio**: Path Traversal |
| GET | `/api/lab/ping` | No | **Laboratorio**: Command Injection |
| GET | `/api/lab/fetch` | No | **Laboratorio**: SSRF |
| GET | `/api/lab/redirect` | No | **Laboratorio**: Open Redirect |
| GET | `/api/lab/secret` | No | **Laboratorio**: secreto hardcodeado expuesto |
| GET | `/` (Swagger UI en dev) | No | Documentación OpenAPI |

## 5. Flujo de autenticación

1. `POST /api/auth/register` → valida email/username duplicados → BCrypt hash de la contraseña → usuario `Role="User"`, `IsActive=true`.
2. `POST /api/auth/login` → busca por email → verifica BCrypt → emite JWT HS256 con claims
   `NameIdentifier` (userId), `Name`, `Email`, `Role`; expira según `Jwt:ExpirationInMinutes` (30).
3. El SPA guarda el token en `localStorage` y lo adjunta como `Authorization: Bearer <token>`.
4. `GET /api/user/profile` devuelve el perfil parseando los claims del token.
5. Logout: solo cliente — el SPA borra `localStorage` (no existe `POST /api/auth/logout`; el JWT
   permanece válido hasta expirar).

## 6. Modelo de datos

- **User**: `Id (Guid)`, `Username` (unique), `Email` (unique), `PasswordHash`, `FirstName`, `LastName`,
  `Role`, `RefreshToken`, `RefreshTokenExpiry`, `IsActive`, `CreatedAt`, `UpdatedAt`.
- **TaskItem**: `Id (Guid)`, `Title`, `Description`, `Status` (Pending/InProgress/Completed),
  `Priority` (Low/Medium/High), `CreatedByUserId → User`, `AssignedToUserId? → User`,
  `DueDate?`, `CreatedAt`, `UpdatedAt`.
- Relaciones: `TaskItem.CreatedByUserId` y `TaskItem.AssignedToUserId` con `OnDelete=Restrict`.
- `AppDbContext` aplica `Migrate()` en cada inicio y hace **seed** de 3 usuarios demo
  (`juan@gmail.com`, `prueba@gmail.com`, `admin@gmail.com`) y 4 tareas demo para Juan.

## 7. Flujo de usuario

1. Registro (o login con usuarios demo).
2. Lista de tareas (`GET /api/taskitem`), filtro por prioridad en el SPA.
3. Crear tarea (envía `createdByUserId` desde el cliente).
4. Editar tarea (ruta `/tasks/edit/:id`, `PUT /api/taskitem/{id}`).
5. Eliminar tarea.
6. Logout (borra el token).

## 8. Posibles superficies de ataque (hallazgos de la línea base)

| # | Severidad | Hallazgo | Ubicación |
|---|---|---|---|
| 1 | **Crítica** | **Broken Object Level Authorization (IDOR)**: `GetAllAsync` devuelve TODAS las tareas de TODOS los usuarios; `GetById/Update/Delete` no validan titularidad. Un usuario puede ver/modificar/borrar tareas ajenas. | `Services/TaskItemService.cs` |
| 2 | Alta | El cliente decide `CreatedByUserId` (DTO) → se puede crear una tarea en nombre de otro usuario. | `DTOs/TaskItem/TaskItemCreateDto.cs`, `Controllers/TaskItemController.cs` |
| 3 | Media | Secret JWT por defecto hardcodeado en `appsettings.json` (placeholder predecible). | `appsettings.json` |
| 4 | Media | Endpoints de laboratorio vulnerables (XSS, SQLi, path traversal, cmd injection, SSRF, open redirect, secrets) **activos por defecto** en main. | `Controllers/LabController.cs` |
| 5 | Media | `LabVulnerabilitiesService.cs` con patrones vulnerables (MD5, secret hardcodeado, SQLi, XSS, path traversal) dentro del proyecto compilable. | `Services/LabVulnerabilitiesService.cs` |
| 6 | Media | No existe `POST /api/auth/logout`; el token no se revoca. | `AuthController.cs` |
| 7 | Baja | `User.RefreshToken` y `RefreshTokenExpiry` existen en el modelo pero no se usan. | `Models/User.cs` |
| 8 | Baja | Token JWT en `localStorage` (XSS del SPA podría robarlo); no hay cookies `HttpOnly`. | `SecureDevOps.Web/src/auth` |
| 9 | Baja | CORS permite cualquier origen `.vercel.app` con cualquier header/método. | `Program.cs` |
| 10 | Info | `.env.lab` con secretos ficticios versionados (INTENCIONAL para Snyk Secrets, pero confirma que el repo no está "limpio" de secrets). | `SecureDevOps.API/.env.lab` |
| 11 | Info | `Dockerfile` de raíz corre como root y SIN healthcheck. | `Dockerfile` |
| 12 | Info | No hay tests automatizados de seguridad. | — |
| 13 | Info | No hay `infrastructure/`, `snyk-api/`, `ai-agent/`, `.github/workflows`. | — |

## 9. Elementos que pueden utilizarse para demostrar cada producto de Snyk

| Producto Snyk | Componente del proyecto que lo demuestra |
|---|---|
| **Snyk Code** | `SnykLab/SnykLabCode/*.cs`, `SecureDevOps.API/Services/LabVulnerabilitiesService.cs`, `SecureDevOps.API/Controllers/LabController.cs` |
| **Snyk Open Source** | `SecureDevOps.Web/package.json` (lodash/minimist/yargs-parser), `SecureDevOps.API/*.csproj` (Newtonsoft.Json/log4net) |
| **Snyk Secrets** | `SecureDevOps.API/.env.lab`, `SnykLab/secrets/*.txt`, secret hardcodeado en `LabVulnerabilitiesService.cs` |
| **Snyk Container** | `Dockerfile` (raíz), `SnykLab/containers/Dockerfile.insecure-*` |
| **Snyk IaC** | `SnykLab/terraform/main.tf`, `SnykLab/terraform/kubernetes.tf` |
| **Snyk API & Web / DAST** | `LabController.cs` (endpoints HTTP vulnerables), `Jenkinsfile` (`snyk monitor`) |
| **Snyk API (REST)** | `Jenkinsfile` (usa `snyk monitor`); pendiente crear scripts con REST API de Snyk (`GET /rest/orgs`, etc.) |
| **Evo / Agentic** | El propio repositorio como base para el ciclo PLAN→IMPLEMENT→TEST→SCAN→REMEDIATE→REPORT |
| **MCP** | No existe aún; se documentará en `ai-agent/MCP.md` |

## 10. Cómo se ejecuta localmente (línea base)

```bash
# Backend (http https://localhost:7196 + Swagger)
cd SecureDevOps.API && dotnet run --urls "https://localhost:7196"

# Frontend (http://localhost:3000, proxy /api → https://localhost:7196)
cd SecureDevOps.Web && npm install && npm run dev
```

- La BD SQLite se crea automáticamente con `Migrate()` y se siembra con usuarios demo
  (`juan@gmail.com` / `contra1234`).

## 11. Nota sobre `snyk-complete-demo/`

Directorio **no versionado** (`git status` → untracked) con una demo distinta (tienda en línea con
Next.js + Orders/Products/Reviews). No forma parte de la solución `.slnx` ni de los despliegues. Se
mantiene intacto; los trabajos de este laboratorio se concentran en la app Task Manager.