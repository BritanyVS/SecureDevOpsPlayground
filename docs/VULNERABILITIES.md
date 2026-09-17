# Vulnerabilidades implementadas (a propósito) — SecureDevOpsPlayground

> Laboratorio Snyk de un solo proyecto: **backend .NET 8** (`SecureDevOps.API`) +
> **frontend React** (`SecureDevOps.Web`) + **SQLite** (volumen). App funcional con
> vulnerabilidades **controladas y fáciles de resolver**, demostrables con los productos
> Snyk: **Code, Open Source, Secrets, IaC y Container**, además de endpoints para
> **Snyk API & Web (DAST)**.

## Cómo levantar la app

```bash
docker compose up --build
# Frontend: http://localhost:3000   Backend API: http://localhost:8081
# Usuarios seed: juan@gmail.com / contra1234 · admin@gmail.com / Admin123!
```

## Cómo correr cada escaneo

```bash
snyk code test                # Snyk Code (SAST)
snyk test --all-projects      # Snyk Open Source
snyk secrets scan .           # Snyk Secrets (fixtures en secrets.example.json)
snyk iac test iac/main.tf     # Snyk IaC
snyk container test snyk-lab-backend --file=Dockerfile   # Snyk Container (imagen local)
```

---

## 1. Snyk Code (SAST) — `SecureDevOps.API/`

| # | Vulnerabilidad | CWE | Archivo / punto exacto | Señal Snyk esperada | Cómo resolver (fácil) |
|---|---|---|---|---|---|
| C1 | **SQL Injection** | CWE-89 | `Services/TaskItemService.cs` → `SearchAsync` (`FromSqlRaw` con `%{query}%`) | "SQL Injection" (dataflow q → SQL) | Usar LINQ: `Where(t => t.Title.Contains(query))` |
| C2 | **Reflected XSS** | CWE-79 | `Controllers/LabController.cs` → `GET /api/lab/xss` (input sin escapar en `text/html`) | "Reflected XSS" / "Improper Neutralization of Input During Web Page Generation" | Devolver texto plano o `HtmlEncoder.Encode(input)` |
| C3 | **Command Injection** | CWE-78 | `Controllers/LabController.cs` → `GET /api/lab/ping` (host interpolado en `/bin/sh -c`) | "Command Injection" | Validar con regex (`^[a-z0-9.-]+$`) o lista blanca |
| C4 | **Path Traversal** | CWE-22 | `Controllers/LabController.cs` → `GET /api/lab/file` (`Path.Combine(uploads, name)`) | "Path Traversal" / "Improper Limitation of a Pathname" | `Path.GetFileName(name)` y comprobar prefijo base |
| C5 | **SSRF** | CWE-918 | `Controllers/LabController.cs` → `GET /api/lab/fetch` (`HttpClient.GetStringAsync(url)`) | "Server-Side Request Forgery" | Validar URL contra lista blanca de dominios y bloquear IPs privadas |
| C6 | **Credenciales hardcodeadas** | CWE-798 | `AppSecrets.cs` (constantes AWS/DB/JWT); `appsettings.json` → `Jwt:SecretKey`; `GET /api/lab/secret` las expone | "Hardcoded credentials" + "Secret in HTTP response" | Leer de variables de entorno / secret manager |
| C7 | **Broken Object Level Authorization (IDOR)** | CWE-639 | `Services/TaskItemService.cs` (todas las consultas) + `Controllers/TaskItemController.cs` (usa `dto.CreatedByUserId`) | "Improper Authorization" / "Broken Access Control" | Filtrar por `ClaimTypes.NameIdentifier` del token |
| C8 | **Stored XSS** | CWE-79 | `Services/TaskItemService.cs` → `CreateCommentAsync` (el `Content` se guarda sin sanitizar y se sirve en el JSON) | "XSS Stored" / "Improper Neutralization of Input" | Sanitizar (HtmlSanitizer) o tratar contenido como texto plano |
| C9 | **SQL Injection (ORDER BY)** | CWE-89 | `Controllers/DashboardController.cs` → `GET /api/dashboard/stats` (`recentOrder` interpolado en `ORDER BY {recentOrder}`) | "SQL Injection" (dataflow recentOrder → SQL) | Lista blanca de columnas + dirección valida |
| C10 | **Missing authz / info disclosure** | CWE-862 / CWE-200 | `Controllers/UsersController.cs` y `Controllers/AuditController.cs` (cualquier usuario autenticado lista emails y actividad) | "Missing Access Control" / "Information Exposure" | `[Authorize(Roles = "Admin")]` y redactar campos |
| C11 | **Data exposure (export)** | CWE-200 | `Controllers/TaskItemController.cs` → `GET /api/taskitem/export` (todas las tareas, sin filtro por usuario ni límite) | "Information Exposure" / "Missing Authorization" | Filtrar por usuario y limitar/redactar |

*Fácil de resolver = cada fix es 1–3 líneas y no rompe la funcionalidad.*

---

## 2. Snyk Open Source — `SecureDevOps.Web/package.json`

Versiones ANCIANAS de paquetes con CVEs conocidos que Snyk Open Source detecta al
escanear `--all-projects` (y que npm reporta en el build → **5 vulns: 4 high, 1 critical**):

| Paquete | Versión (actual en el repo) | Hallazgo esperado | Fix |
|---|---|---|---|
| `lodash` | `^4.17.15` | Prototype Pollution (CVE-2019-10744, CVE-2021-23337) | `^4.17.21` (o última) |
| `minimist` (dev) | `^1.2.0` | Prototype Pollution (CVE-2020-7598, CVE-2021-44906) | `^1.2.8` |
| `yargs-parser` (dev) | `^5.0.1` | Prototype Pollution (CVE-2020-7608) | Última 5.x/21.x |

---

## 3. Snyk Secrets — `secrets.example.json` + código

| Fixture | Valor (ficticio) | Regla Snyk Secrets |
|---|---|---|
| AWS Access Key | `AKIAIOSFODNN7EXAMPLE` | AWS access key |
| AWS Secret Key | `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY` | AWS secret key |
| DB password | `P@ssw0rd_Demo_123!` | Generic secret |
| JWT secret | `SuperSecretJwtKeyHardcoded_Demo_123!` | Generic secret |
| Private key | `-----BEGIN PRIVATE KEY-----…` | Private key |

También detecta las constantes de `AppSecrets.cs` y `appsettings.json`.
Fix: borrar fichero + pasar credenciales por variables de entorno; rotar si se filtraron.

---

## 4. Snyk IaC — `iac/main.tf`

| # | Hallazgo | Regla Snyk IaC esperada | Fix |
|---|---|---|---|
| I1 | S3 con ACL `public-read` | `S3 Bucket ACL All Users Read` | `acl = "private"` + bloqueo de acceso público |
| I2 | SG abierto `0.0.0.0/0` en 22 y 3306 | `SSH Port Open to Internet`, `MySQL open to the internet` | CIDR restringido / referenciar SG |
| I3 | RDS `storage_encrypted=false` | `RDS Storage Unencrypted` | `storage_encrypted = true` |
| I4 | RDS `backup_retention_period=0` | `RDS Backup Disabled` | `>= 7` |
| I5 | RDS `publicly_accessible=true` | `RDS Publicly Accessible` | `false` + subredes privadas |
| I6 | RDS con password hardcodeada | `PlainText Password` | variables `sensitive` |
| I7 | IAM `Action:["*"]` / `Resource:["*"]` | `IAM policy grants all access` | policies de menor privilegio |
| I8 | Credenciales en el provider | `PlainText` (AWS keys) | `~/.aws/credentials` / OIDC |

---

## 5. Snyk Container — `Dockerfile` (backend)

| Hallazgo | Regla Snyk Container esperada | Fix |
|---|---|---|
| Runtime = imagen `dotnet/sdk:8.0` (trae build tools y más CVEs) | CVEs de alta gravedad en la imagen base (Debian bookworm + SDK) | Multi-stage con `aspnet:8.0` como runtime |
| Ejecución como **root** | "Image runs as root user" | `USER app` (imagen aspnet trae `app`) |
| Sin `HEALTHCHECK` | Recomendación de base | `HEALTHCHECK CMD curl -fs http://localhost:8080/health` |

---

## 6. Snyk API & Web (DAST) — App en ejecución

Los escáneres dinámicos (Snyk API & Web) descubren estas rutas en el frontend y las
prueban con payloads:

| Endpoint | Método | Descripción / payload sugerido |
|---|---|---|
| `/api/auth/register` | POST | Body `{"username","email","password","firstName","lastName"}` |
| `/api/auth/login` | POST | Body `{"email","password"}` → token JWT |
| `/api/auth/logout` | POST | Cierre de sesión |
| `/api/user/profile` | GET | Perfil autenticado |
| `/api/taskitem` | GET/POST | Listar / crear tareas |
| `/api/taskitem/{id}` | GET/PUT/DELETE | Tarea por id (IDOR: juega con ids ajenos) |
| `/api/taskitem/{id}/status` | PATCH | Cambio de estado (más verbos HTTP para la demo de API) |
| `/api/taskitem/{id}/comments` | GET/POST | Comentarios; POST con `<script>` en `content` (stored XSS) |
| `/api/taskitem/search?q=` | GET | SQLi: `q=' UNION SELECT ...--` |
| `/api/taskitem/export?format=csv` | GET | Exporta todas las tareas (data exposure) |
| `/api/dashboard/stats?recentOrder=` | GET | Stats; `recentOrder=<expr SQL>` (ORDER BY injection) |
| `/api/users` · `/api/users/{id}` | GET | Lista usuarios y emails (authz / data exposure) |
| `/api/audit/logs` | GET | Actividad con emails (authz / info leak) |
| `/api/lab/xss?input=` | GET | `<script>alert(1)</script>` |
| `/api/lab/search?q=` | GET | `q=' OR '1'='1` |
| `/api/lab/file?name=` | GET | `name=../../etc/passwd` |
| `/api/lab/ping?host=` | GET | `host=localhost;whoami` |
| `/api/lab/fetch?url=` | GET | `url=http://169.254.169.254/latest/meta-data/` |
| `/api/lab/redirect?url=` | GET | `url=https://evil.com` |
| `/api/lab/secret` | GET | Expone secretos hardcodeados |
| `/health` | GET | Healthcheck (público, sin auth) |

## Resumen (tabla única)

| ID | Producto Snyk | Archivo | Gravedad esperada |
|---|---|---|---|
| C1 | Code | TaskItemService.cs | High |
| C2 | Code | LabController.cs (xss) | Medium/High |
| C3 | Code | LabController.cs (ping) | High |
| C4 | Code | LabController.cs (file) | Medium/High |
| C5 | Code | LabController.cs (fetch) | Medium |
| C6 | Code/Secrets | AppSecrets.cs, appsettings.json | High/Medium |
| C7 | Code | TaskItemService/Controller | High |
| C8 | Code | TaskItemService.cs (comments) | Medium/High |
| C9 | Code | DashboardController.cs (ORDER BY) | High |
| C10 | Code | UsersController.cs, AuditController.cs | Medium |
| C11 | Code | TaskItemController.cs (export) | Medium |
| O1–O3 | Open Source | package.json (lodash, minimist, yargs-parser) | High/Critical |
| S1–S5 | Secrets | secrets.example.json | High/Medium |
| I1–I8 | IaC | iac/main.tf | High/Critical |
| D1–D4 | Container | Dockerfile | High + recomendaciones |
| — | API & Web | Endpoints `/api/*`, `/api/lab/*` | Array (DAST) |