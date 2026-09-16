# Prompt: Revisar endpoints/API para asegurar (DAST-friendly)

```text
Revisa la seguridad de la API REST de SecureDevOps.API (.NET 8).

REVISA
- Controllers: AuthController, TaskItemController, LabController, UserController.
- Services: AuthService, TaskItemService, JwtService.
- Uso de claims (`ClaimTypes.NameIdentifier`) y `[Authorize]`/roles.

QUE VERIFICAR (y documentar, sin cambiar aún)
1. Autenticación: login con BCrypt, token JWT con HS256, expiración.
2. Autorización: ¿cada endpoint valida que el recurso pertenece al usuario del token? (IDOR).
3. Input validation: DataAnnotations/`[ApiController]`, deserialización de JSON.
4. Errores: no filtrar stack traces ni excepciones internas (dev vs prod).
5. Headers de seguridad en respuestas.
6. Rate limiting / brute force en login (actual: sin rate-limit → anotar).
7. CORS correcto (solo orígenes de la app).
8. `/api/lab/*`: solo accesibles con `SecurityLab__Enabled=true` (demo).

ENTREGABLE
- Tabla: endpoint | riesgo | estado (ok / mejora / pendiente) | mitigación.
- Verifica con `curl` casos de IDOR (ver docs/DAST-SCENARIOS.md).
- Si haces cambios: `dotnet test` y `npm run build` ANTES y DESPUÉS + `snyk code test`.
```