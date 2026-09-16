# 02 — Command Injection

## Código vulnerable
Ver `command-injection.cs` (método `PingHost`).

```csharp
var psi = new ProcessStartInfo("/bin/sh", $"-c \"ping -c 1 {host}\"") { ... };
```

## Vulnerabilidad esperada
- **Snyk Code**: `Command Injection` (Severity: High / Critical).
- El input `host` fluye (dataflow) hacia `ProcessStartInfo` con argumentos a través del shell
  (`UseShellExecute`/`-c`), sin sanitización.

## Impacto
- Ejecución arbitraria de comandos en el servidor (RCE).
- Compromiso total del contenedor/host, robo de secretos, movimiento lateral.

## Remediación
1. **Nunca** pasar input del usuario al shell. Usar `ProcessStartInfo.ArgumentList` (sin shell).
2. Validar contra una whitelist / regex estricta.
3. Si no hay alternativa segura, prohibir el patrón (SAST blocker).

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir, el finding Command Injection desaparece para `PingHostSecure`.
```