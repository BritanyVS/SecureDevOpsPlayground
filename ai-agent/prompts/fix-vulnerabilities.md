# Prompt: Remediar vulnerabilidades de Snyk

```text
Actúa como experto en remediación de Snyk. Siguiendo el ciclo agentic, corrige los
findings del escaneo sin romper tests ni funcionalidad.

PROCESO
1. ESCANEA: `snyk test --all-projects --json` y `snyk code test --json` (y `snyk container` si aplica). Anota severity/regla/path del paquete.
2. PRIORIZA por severidad y por si el paquete es de producción o dev.
3. REMEDIA (no apliques fixes a ciegas):
   - Versiones seguras: comprueba que el update no rompe APIs usadas (revisa los imports en el repo).
   - Prefiere el upgrade mínimo viable; usa `snyk test --dev` para dev deps.
   - Si Snyk sugiere `snyk fix` o `snyk wizard`, evalúalo manualmente.
   - No cambies versiones solo porque Snyk lo diga si rompe el build; documenta el trade-off.
4. VERIFICA: re-ejecuta `dotnet test` + `npm run build` + `snyk test`/`snyk code test`.
5. REPORTEA por cada vuln:
   - antes/después (severidad, paquete@versión)
   - cambio hecho (package.json / .csproj / lockfile)
   - si no se puede corregir → mitigación y justificación.

⚠ Aplica cambios solo a manifests y código del repo; no inventes reglas ni paquetes.
```