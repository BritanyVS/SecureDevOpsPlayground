# 04 — Cross-Site Scripting (XSS)

## Código vulnerable
Ver `xss.cs` (métodos `RenderUserInputHtml` y `RenderReactCard`).

```csharp
return $"<html><body><p>Resultados para: {input}</p></body></html>";
```

## Vulnerabilidad esperada
- **Snyk Code**: `Cross-site Scripting (XSS)` (Severity: Medium/High).
- Snyk detecta el flujo input de usuario → respuesta HTML sin sanitización (`text/html`).

## Impacto
- Robo de sesión (cookie `HttpOnly` mitiga pero el token en `localStorage` se roba igual).
- Suplantación de usuario, keylogging, phishing dentro de la app.
- Para el DAST: `GET /api/lab/xss?input=<script>...` devuelve HM en `text/html`.

## Remediación
1. `WebUtility.HtmlEncode` / `SecurityElement.Escape` al interpolar HTML en el backend.
2. En el frontend: nunca usar `dangerouslySetInnerHTML`/`v-html` con input del usuario.
3. Headers de seguridad: `Content-Security-Policy`, `X-Content-Type-Options: nosniff`.

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`HtmlEncode`), el finding XSS desaparece.
```