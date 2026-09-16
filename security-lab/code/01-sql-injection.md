# 01 — SQL Injection

## Código vulnerable
Ver `sql-injection.cs` (método `SearchUsers`).

```csharp
using var cmd = new SqlCommand($"SELECT Username FROM Users WHERE Username LIKE '%{q}%'", conn);
```

## Vulnerabilidad esperada
- **Snyk Code**: `SQL Injection` (Severity: High / Critical).
- Motor: Snyk Google DeepCode informa que `q` es dataflow alcanza una consulta SQL sin
  parametrizar (sink `SqlCommand`) desde un origen externo (parámetro de método / HTTP).

## Impacto
- Lectura de toda la base de datos (tablas, credenciales, PII).
- Escritura de datos (`INSERT`/`UPDATE`/`DELETE`) y ejecución de funciones (`xp_cmdshell`).
- Potencial ejecución de código en el servidor SQL.

## Remediación
1. **Parametrizar siempre** las consultas (`AddWithValue` / `SqlParameter`).
2. Usar ORM (EF Core) con LINQ: los valores se pasan como parámetros automáticamente.
3. Principio de menor privilegio para el usuario de BD y firewall de red.

## Cómo verificar
```bash
snyk code test security-lab/code
# After fix:
# Snyk Code no reporta más el finding SQL Injection para `SearchUsersSecure`.
```