# DAST — Escenarios para Snyk API & Web (pruebas dinámicas)

Guía para ejecutar **escaneos dinámicos** (Snyk API & Web / DAST) contra la aplicación
desplegada o local. Requisitos:

1. App corriendo (ver `docs/SNYK-DEMO-GUIDE.md` → "Cómo desplegar para DAST").
2. Para los endpoints de laboratorio: habilitar `SecurityLab__Enabled=true`.
3. URL base: se usa `https://localhost:7196` (dev) o `http://localhost:8080` (docker).

> Para escaneo con Snyk API & Web: añade el proyecto Domain/URL del entorno desde la consola
> Snyk (AppRisk). Configura la URL de login para el "Authenticated scan" cuando aplique.

## Autenticación para el scan

La app usa JWT en `Authorization` header. El escáner puede usar credenciales de login
(los usuarios demo seed están disponibles) para el escaneo autenticado:

- `juan@gmail.com` / `contra1234`
- `prueba@gmail.com` / `contra1234`
- `admin@gmail.com` / `Admin123!`

## Escenarios manuales (curl) recomendados

### 1. Login
```bash
curl -k -X POST https://localhost:7196/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"juan@gmail.com","password":"contra1234"}'
# Esperado: 200 + { token, expiresAt }
```

### 2. Acceso sin autenticación
```bash
curl -k https://localhost:7196/api/taskitem
# Esperado: 401 Unauthorized (sin token)
```

### 3. Acceso a tarea propia
```bash
TOKEN=$(curl -sk https://localhost:7196/api/auth/login -H "Content-Type: application/json" \
  -d '{"email":"juan@gmail.com","password":"contra1234"}' | python -c "import sys,json;print(json.load(sys.stdin)['token'])")

curl -k https://localhost:7196/api/taskitem -H "Authorization: Bearer $TOKEN"
# Esperado: 200 con SOLO las tareas de Juan (IDs seed). Anotar un taskId.
```

### 4. Intento de acceder a tarea ajena (IDOR)
```bash
# login como otro usuario y probar el taskId de Juan:
TOKEN_OTHER=$(curl -sk .../login -d '{"email":"prueba@gmail.com","password":"contra1234"}'...)

curl -k https://localhost:7196/api/taskitem/<taskId-de-juan> -H "Authorization: Bearer $TOKEN_OTHER"
curl -k -X DELETE https://localhost:7196/api/taskitem/<taskId-de-juan> -H "Authorization: Bearer $TOKEN_OTHER"
# CORRECTO (post-fix): 404 en GET/PUT/DELETE y la tarea sigue existiendo para Juan.
```

### 5. Manipulación de IDs
- `GET /api/taskitem/00000000-0000-0000-0000-000000000000` → 404 (no revela existencia).
- `GET /api/taskitem/no-es-un-guid` → 400/404 (ruta `{id:guid}`).

### 6. Inputs inválidos
- `POST /api/taskitem` sin body → `400` / `422`.
- Título vacío → `400`.
- `POST /api/auth/register` con email inválido → `400`.

### 7. Payloads maliciosos (lab habilitado)
```bash
# XSS reflejado
curl -k "https://localhost:7196/api/lab/xss?input=%3Cscript%3Ealert(1)%3C/script%3E"

# SQL Injection
curl -k "https://localhost:7196/api/lab/search?q='%20UNION%20SELECT%20Username,Email%20FROM%20Users--"

# Path Traversal
curl -k "https://localhost:7196/api/lab/file?name=../../../../etc/passwd"

# Command Injection
curl -k "https://localhost:7196/api/lab/ping?host=localhost;id"

# SSRF contra metadata
curl -k "https://localhost:7196/api/lab/fetch?url=http://169.254.169.254/latest/meta-data/"

# Open Redirect
curl -k -I "https://localhost:7196/api/lab/redirect?url=https://evil.example.com"

# Secrets expuestos
curl -k "https://localhost:7196/api/lab/secret"
```

### 8. Headers de seguridad
- Verificar que existen: `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`,
  `Referrer-Policy` (nginx de producción los añade; para la API considerar añadirlos en
  middleware si la demo lo requiere).

### 9. Cookies / tokens
- Verificar que el token no va en cookies; está en `localStorage` + `Authorization` header
  (documentar: en un entorno real de producción se prefiere cookie `HttpOnly`, `Secure`,
  `SameSite` o token corto y rotación).
- Token manipulado / expirado → `401`.

### 10. Manejo de errores
- Endpoints inexistentes → `404` JSON (no stack traces).
- En desarrollo se muestra el error developer-friendly; en `Production` NO exponer
  detalles internos (los datos de exception están desactivados por defecto en Release).

## Crawling / "Discovery page"

La SPA tiene una página `/lab` que lista los endpoints de laboratorio para que el crawler
de Snyk API & Web descubra sus URLs. La ruta real del backend para cada endpoint es
`/api/lab/{xss|search|file|ping|fetch|redirect|secret}`.

## Interpretación de resultados del DAST

- Cada finding se mapea a OWASP/CWE y a una solicitud HTTP concreta.
- Verifica **falso positivo** reproduciendo el request manualmente.
- La remediación de código correspondiente está en `security-lab/code/*.md`.
- Tras remediar, re-escanea y confirma que el issue desaparece del proyecto Snyk AppRisk.