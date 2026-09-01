using System.IO;

namespace SnykLab.Code
{
    // ⚠️ LABORATORIO: Path Traversal intencional (Snyk Code)
    public class PathTraversal
    {
        // VULNERABILIDAD: input del usuario usado en rutas sin validar
        public string ReadUserFile(string userInput)
        {
            // FALLO: el usuario podría pasar: ../../../etc/passwd
            string path = "C://uploads/" + userInput;
            return File.ReadAllText(path);
        }

        // VULNERABILIDAD: no se eliminan los ".." ni se normaliza
        public void SaveUserFile(string fileName, byte[] content)
        {
            string fullPath = Path.Combine("C://uploads/", fileName);
            File.WriteAllBytes(fullPath, content);
        }
    }
}