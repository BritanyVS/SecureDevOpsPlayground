# SECURITY-ARCHITECTURE — Arquitectura de seguridad de SecureDevOpsPlayground

## 1. Modelo de datos y usuarios

- **Usuarios** (`Users`): id (GUID), username, email, passwordHash (BCrypt.Net),
  firstName/lastName, role (`User`|`Admin`), isActive, createdAt, refreshToken* ,
  refreshTokenExpiryTime*.  **No se almacenan contraseñas en claro.**
- **Tareas** (`TaskItems`): id, title, description, status, priority, dueDate,
  createdByUserId, assignedToUserId, timestamps.
- Todas las tareas/propiedades se filtran **por usuario autenticado**.

## 2. Autenticación (JWT Bearer)

- Flujo login: verifica `isActive` → `BCrypt.Verify` → emite JWT (HS256, expires ~30 min).
- Claims: `NameIdentifier` (userId), `Name` (userName), `Role`, `Email`.
- **Secreto**: `JWT_SECRET` (env, tiene precedencia) o `Jwt:SecretKey` (appsettings).
  Requerido: si ambos vacíos → la app lanza excepción (no arranca con clave débil).
- Logout: `POST /api/auth/logout` (token ya no se usa localmente; el cliente borra storage).
  Nota: en un entorno con crítica de revocación inmediata, añadir blocklist/denylist de
  tokens o JWT de corta duración + refresh rotativo.

## 3. Autorización y control de acceso (la pieza central)

- Cada método de `TaskItemService` recibe `currentUserId` (del claim) y filtra:
  `q = q.Where(t => t.CreatedByUserId == currentUserId)`.
- `TaskItemController` obtiene id desde `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- GET/PUT/DELETE de tarea ajena → **404** (no 403): no revela la existencia del recurso.
- `POST /api/taskitem` ignora cualquier `CreatedByUserId` enviado por el cliente.
- Los endpoints `/api/lab/*` se sirven **solo** si `SecurityLab__Enabled=true`; en caso
  contrario devuelven 404 (capa *feature-flag*).

## 4. Protección de datos en reposo

- BD local: SQLite (`SecureDevOpsDb.db`); en Docker se monta en volumen `sqlite-data`.
- Referencia IaC (production-style): RDS MySQL con `storage_encrypted=true`, subredes
  privadas, backups 15 días, `deletion_protection`.
- Ficheros/generated docs: `.gitignore` excluye `.env`, `.db`, claves; fixtures de
  secretos son ficticios.

## 5. Protección de datos en tránsito

- Dev: HTTPS (dotnet dev cert). En docker-compose el nginx actúa como TLS-reverse proxy
  del frontend y del `/api`.
- Headers de seguridad aplicados en nginx (`X-Content-Type-Options`, CSP, etc.).

## 6. Seguridad de la aplicación (OWASP ASVS mapeo)

| Riesgo | Mitigación en el repo | Ver demo |
|---|---|---|
| Broken Access Control / IDOR | propietario desde claims; 404 en ajeno | `security-lab/code/09-*`, `docs/DAST-SCENARIOS.md` #4 |
| Injection (SQL/XSS/Command) | `security-lab` demos; en producción: EF LINQ, Razor/Vite escapan outputs | `security-lab/code/01,02,05` |
| Failed AuthN (passwords) | BCrypt, campos de error genéricos | `security-lab/code/*` + tests |
| Secrets / leak | `JWT_SECRET` por env; `.gitignore`; fixtures ficticios | `security-lab/secrets` |
| SSRF / Open Redirect | demos aislados en `/api/lab/*`; código de la app real lo previene | `security-lab/code/06,07` |
| Deserialización insegura | no se reciben objetos serialized de cliente | `security-lab/code/04` |
| Logging/Monitor | `/health`, CloudWatch en IaC | `infrastructure/terraform` |

## 7. Pipeline / CI-CD

- GitHub Actions (`ci.yml`): build + tests + escaneos Snyk con SARIF y secret
  `SNYK_TOKEN`. Se garantiza que **ningún token** viva en el YAML.
- Jenkinsfile heredado: referencia imágenes y `SnykLab`; mantener como muestra.

## 8. Contenedores

- `docker/Dockerfile.backend`: multi-stage, **usuario no-root** `app`, HEALTHCHECK con
  curl, SQLite en `/app/data` (volumen), `ASPNETCORE_URLS=http://+:8080`.
- `docker/Dockerfile.frontend`: build node → `nginxinc/nginx-unprivileged:1.27-alpine`
  (no-root), copia `nginx.conf` de `docker/`.
- Principios: minimizar superficie (instalar solo lo necesario, limpiar cache),
  base pinned/verified, sin secrets en imágenes.

## 9. Tecnología clave

- .NET 8 (ASP.NET Core Web API), EF Core 8 + SQLite, JWT Bearer HS256, BCrypt.Net-Next.
- React 19 (Vite/TS), Axios; axiosInterceptor añade Bearer y desloguea en 401.
- xUnit + EF InMemory (tests).
- AWS IaC de referencia: VPC/NAT/SG/RDS/ALB/ECS Fargate/Secrets Manager/CloudWatch.

## 10. Línea de base y gaps conocidos (honestidad)

- **Rate limiting/login brute-force**: no implementado en app (documentado en DAST; puede
  añadirse en CI o reverse proxy).
- **Revocación de JWT**: logout es best-effort en cliente (stateless); para exigirla →
  blocklist.
- **Toket storage en frontend**: `localStorage` (conveniencia); en producción considerar
  cookie `HttpOnly` + `SameSite`.
- **TLS en docker-compose local**: el proxy sirve HTTP; para "external demo" añadir
  certificados.
- La versión "production-like" de IaC existe como referencia (RDS/ECS) pero el run mode
  local es SQLite/FileSystem.