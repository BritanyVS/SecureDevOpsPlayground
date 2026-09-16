using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: INSECURE DESERIALIZATION / DESERIALIZACIÓN INSEGURA
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class InsecureDeserializationLab
{
    // VULNERABLE: BinaryFormatter deserializa bytes controlados por el atacante
    // → permite construir objetos arbitrarios (gadget chains) → RCE.
    // Solo válido como demo: BinaryFormatter da error en .NET 8.
    public object DeserializeBinary(byte[] payload)
    {
        using var stream = new MemoryStream(payload);
        // FALLO: BinaryFormatter es inseguro para datos no confiables.
        var formatter = new BinaryFormatter();
        return formatter.Deserialize(stream);
    }

    // VULNERABLE: Newtonsoft.Json con TypeNameHandling.All permite polimorfismo arbitrario.
    public object DeserializeJson(string json)
    {
        // FALLO DE CARGA ÚTIL: TypeNameHandling.All + input del usuario → RCE en versiones viejas.
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        return JsonConvert.DeserializeObject(json, settings)!;
    }

    // ✅ CORREGIDO: usar la serialización por defecto sin TypeNameHandling
    // y un tipo concreto conocido, nunca tipos derivados del input.
    public T? DeserializeJsonSecure<T>(string json)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };
        return JsonConvert.DeserializeObject<T>(json, settings);
    }
}