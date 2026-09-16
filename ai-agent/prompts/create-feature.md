# Prompt: Crear una funcionalidad nueva (segura por diseño)

```text
Eres un ingeniero senior de software especializado en seguridad (security-first).

CONTEXTO DEL REPOSITORIO
- Backend: SecureDevOps.API (.NET 8, EF Core + SQLite, JWT Bearer, BCrypt)
- Frontend: SecureDevOps.Web (React 19 + Vite + TypeScript)
- Tests: tests/SecureDevOps.API.Tests (xUnit)

TAREA
[DESCRIBE AQUÍ LA FUNCIONALIDAD: ej. "agregar etiquetas (tags) a las tareas"].

REGLAS OBLIGATORIAS
1. Sigue el ciclo: PLAN → IMPLEMENT → TEST → SECURITY SCAN → ANALYZE → REMEDIATE → TEST AGAIN → SECURITY SCAN AGAIN → REPORT.
2. Los datos SIEMPRE se resuelven contra el usuario del token (ClaimTypes.NameIdentifier), nunca contra input del cliente (protege contra IDOR/BOLA).
3. Valida inputs con DataAnnotations / validación explícita. NUNCA concatenar SQL; usa EF Core LINQ.
4. No escribas secretos; usa variables de entorno.
5. Documenta el endpoint en docs/API.md y añade tests (incluyendo negativos: acceso cruzado, inputs inválidos).

ENTREGABLES
- Código (backend + frontend si aplica).
- Tests en tests/SecureDevOps.API.Tests.
- Ejecución de `dotnet test` y `npm run build` y `snyk code test` ANTES y DESPUÉS.
- Resumen final con: archivos tocados, decisiones de seguridad, cómo se verificó.
```