# 05 — Insecure Deserialization

## Código vulnerable
Ver `insecure-deserialization.cs`.

```csharp
var formatter = new BinaryFormatter();
return formatter.Deserialize(stream);

// o: Newtonsoft con TypeNameHandling.All
var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
return JsonConvert.DeserializeObject(json, settings)!;
```

## Vulnerabilidad esperada
- **Snyk Code**: `Insecure Deserialization` (Severity: High/Critical).
- Detecta `BinaryFormatter`/`LosFormatter`/`ObjectStateFormatter` y `TypeNameHandling != None`
  con entrada no confiable.

## Impacto
- Ejecución remota de código (gadget chains tipo `ObjectDataProvider`, `TypeConfuseDelegate`).
- En .NET moderno: DoS/abuso de constructores, lectura de archivos.

## Remediación
1. **No deserializar datos no confiables**. Usar formatos no ejecutables: JSON/XML/texto plano.
2. Si se necesita Newtonsoft: `TypeNameHandling.None` (por defecto).
3. Nunca exponer `ObjectStateFormatter`/`BinaryFormatter` a tráfico HTTP (obsoleto en .NET 8+).

## Cómo verificar
```bash
snyk code test security-lab/code
# Tras corregir (`DeserializeJsonSecure`), Snyk no reporta el finding.
```