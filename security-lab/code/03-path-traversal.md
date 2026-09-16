# 03 — Path Traversal

## Código vulnerable
Ver `path-traversal.cs` (método `ReadFile`).

```csharp
var path = Path.Combine(_baseDir, fileName);   // fileName = ../../etc/passwd
return File.ReadAllText(path);
```

## Vulnerabilidad esperada
- **Snyk Code**: `Path Traversal` (Severity: High).
- Snyk rastrea `fileName` desde el controlador HTTP hasta `File.ReadAllText` y detecta que
  `Path.Combine` + `..` permite escapar del directorio base.

## Impacto
- Lectura de archivos arbitrarios del servidor (config, claves, credenciales, `/etc/passwd`).
- En combinación con escritura, da lugar a RCE.

## Remediación
1. Normalizar con `Path.GetFullPath` y verificar `StartsWith(baseDir)`.
2. Bloquear `..`, `~`, rutas absolutas y caracteres de control.
3. Preferir servir archivos por su `Id` (mapping a un hash) en lugar de por nombre.

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`ReadFileSecure`), Snyk no reporta Path Traversal.
```