# API — Documentación de la API REST

Base URL (dev): `https://localhost:7196` — Base URL (docker): `http://localhost:8080`
Formato general: JSON. Auth: `Authorization: Bearer <token>` excepto donde se indique.

> Versión interactiva: Swagger UI en `Development` (`/swagger`).

## Convenciones

- Errores: códigos HTTP estándar con detalle en el cuerpo.
- `401` = no autenticado. `403` = autenticado pero sin permiso. `404` = recurso inexistente
  **o no perteneciente al usuario** (evita leak de existencia).
- Las tareas SIEMPRE se resuelven contra el **usuario del token** (id de `ClaimTypes.NameIdentifier`).
  El campo `createdByUserId` que manda el cliente se ignora en creación.

---

## Auth

### POST /api/auth/register
Registra un usuario.

- Headers: `Content-Type: application/json`
- Body:
```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "Str0ngPassw0rd!",
  "firstName": "Alice",
  "lastName": "Liddell"
}
```
- Response `201 Created`:
```json
{
  "id": "9f8c...",
  "username": "alice",
  "email": "alice@example.com",
  "role": "User",
  "firstName": "Alice",
  "lastName": "Liddell",
  "isActive": true,
  "createdAt": "2026-09-16T10:00:00Z"
}
```
- Errores: `400` con `["Email already exists"]`, `422` si no pasa validación
  (email inválido, password < 8).

### POST /api/auth/login
- Body: `{ "email": "...", "password": "..." }`
- Response `200`:
```json
{ "token": "<jwt>", "expiresAt": "2026-09-16T10:30:00Z" }
```
- Errores: `401 Unauthorized` con credenciales inválidas.

### POST /api/auth/logout
Cierre de sesión (requiere token).

- Requiere `Authorization`.
- Response `204 No Content`. En JWT stateless la revocación inmediata requiere blocklist
  (ver `docs/SECURITY-ARCHITECTURE.md`); el cliente elimina el token local.

### GET /api/user/profile
Perfil del usuario autenticado.

- Requiere `Authorization`.
- Response `200`:
```json
{ "userId": "9f8c...", "username": "alice", "email": "alice@example.com", "role": "User" }
```

---

## Tasks (requieren Authorization)

### GET /api/tasks
> El endpoint real del proyecto es **`GET /api/taskitem`** (mantenido por compatibilidad).

Lista las tareas **del usuario autenticado** (aisladas por usuario).

- Response `200`:
```json
[
  {
    "id": "6ee4...",
    "title": "Configurar CI/CD",
    "description": null,
    "status": "Pending",
    "priority": "Medium",
    "createdByUserId": "9f8c...",
    "assignedToUserName": "alice",
    "dueDate": "2026-09-23T10:00:00Z",
    "createdAt": "2026-09-16T09:00:00Z",
    "updatedAt": "2026-09-16T09:00:00Z"
  }
]
```

### GET /api/taskitem/{id}
Obtiene UNA tarea. Devuelve `404` si no existe o **no pertenece al usuario**.

### POST /api/taskitem
- Body:
```json
{
  "title": "Revisar Snyk",
  "description": "Correr snyk code test",
  "priority": "High",
  "assignedToUserId": "9f8c...",
  "dueDate": "2026-09-20T00:00:00Z"
}
```
- `createdByUserId` opcional: **se ignora**, se fuerza al usuario del token.
- Response `201 Created` con la tarea completa.

### PUT /api/taskitem/{id}
Actualiza título/descripción/status/priority/assignee/dueDate. `404` si no es del usuario.

### DELETE /api/taskitem/{id}
Elimina la tarea. `204 No Content`. `404` si no es del usuario.

---

## Laboratorio (Snyk DAST) — SOLO con `SecurityLab__Enabled=true`

Activación: env `SECURITYLAB__ENABLED=true` (o `appsettings.json` → `SecurityLab:Enabled`).
Con el lab desactivado → `404` en todos.

| Método | Ruta | Vulnerabilidad demostrada |
|---|---|---|
| GET | `/api/lab/xss?input=<script>` | Reflected XSS (`text/html`) |
| GET | `/api/lab/search?q=...` | SQL Injection (SQLite) |
| GET | `/api/lab/file?name=../../...` | Path Traversal |
| GET | `/api/lab/ping?host=...` | Command Injection |
| GET | `/api/lab/fetch?url=...` | SSRF |
| GET | `/api/lab/redirect?url=...` | Open Redirect |
| GET | `/api/lab/secret` | Secretos hardcodeados en respuesta |

---

## Health check

### GET /health
Prueba de vida sin auth. Response `200 {"status":"ok","timestamp":...}`.