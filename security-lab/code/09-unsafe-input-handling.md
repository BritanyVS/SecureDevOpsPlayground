# 09 — Unsafe input handling (ReDoS, header injection)

## Código vulnerable
Ver `unsafe-input-handling.cs`.

```csharp
private static readonly Regex EvilRegex = new(@"^(a+)+$", RegexOptions.Compiled);
return EvilRegex.IsMatch(input);          // ReDoS: backtracking exponencial

return $"Location: {url}";                // Header Injection si url contiene CR/LF
```

## Vulnerabilidad esperada
- **Snyk Code**: `Inefficient Regular Expression` / `ReDoS` (Severity: Medium/High) y
  `HTTP Response Splitting` / `Header Injection`.

## Impacto
- ReDoS: peticiones con input malicioso saturan la CPU → **DoS** de la API.
- Header injection: respuesta dividida → cualquier cabecera arbitraria,
  eventualmente **XSS / cache poisoning**.

## Remediación
1. Simplificar la regex o usar un analizador determinístico (parsers). NUNCA `(a+)+`-style.
2. Si la regex es necesaria: `RegexOptions` + `TimeSpan` timeout (`new Regex(pattern, opts, ts)`).
3. Para cabeceras/URLs: validar contra allowlist y **eliminar CR/LF** antes de usarlas.
4. Aplicar `[ValidateNever]`/`DataAnnotations` en las DTOs del API.

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`*Secure`), Snyk no reporta ReDoS ni header injection.
```