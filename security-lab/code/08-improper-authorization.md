# 08 — Improper Authorization (IDOR / BOLA)

## Código vulnerable
Ver `improper-authorization.cs`. **Este era el bug real de `TaskItemService.cs` en la línea base**
(ver `SECURITY-LAB-BASELINE.md`, hallazgo #1).

```csharp
return await tasks.FirstOrDefaultAsync(t => t.Id == id);   // sin filtro de propietario
```

## Vulnerabilidad esperada
- **Snyk Code**: `Improper Authorization` / `Missing access control` / `IDOR` (Severity: High).
- Snyk detecta que el dato se obtiene solo por `Id` sin comprobar el `userId` autenticado.

## Impacto
- Un usuario logueado accede a tareas, perfiles, pedidos o datos de **otros usuarios**
  cambiando el `id` en la URL. Es el bug de descubrimiento de datos más explotado
  (OWASP API1: Broken Object Level Authorization).

## Remediación
1. **Recuperar siempre el `userId` del token** (`ClaimTypes.NameIdentifier`), nunca del body/query.
2. Filtrar las queries EF/LINQ por `CreatedByUserId == currentUserId`.
3. Añadir tests de aislamiento (ver `tests/SecureDevOps.API.Tests/TaskItemServiceSecurityTests.cs`).

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`GetByIdAsyncSecure`), el finding de autorización desaparece.
dotnet test tests/SecureDevOps.API.Tests   # test de aislamiento verde
```