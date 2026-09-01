using System.Security.Cryptography;
using System.Text;

namespace SnykLab.Code
{
    // ⚠️ LABORATORIO: Criptografía débil intencional (Snyk Code)
    public static class CryptographyWeak
    {
        // VULNERABILIDAD: MD5 es criptográficamente roto
        public static string HashPasswordMd5(string password)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }

        // VULNERABILIDAD: SHA1 es débil para criptografía
        public static string HashDataSha1(string data)
        {
            using var sha1 = SHA1.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha1.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        // VULNERABILIDAD: algoritmo de cifrado débil (DES es inseguro)
        // Este método simula el uso de un cifrado inseguro
        public static string LegacyEncrypt(string value)
        {
            // FALLO: uso de relleno ECB (inseguro) - ejemplo ilustrativo
            return "ENCRYPTED(" + value + ")" + "AES-ECB-INSECURE";
        }

        // VULNERABILIDAD: clave predefinida / hardcodeada
        private static readonly byte[] HardcodedKey = new byte[16] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 };
        public static byte[] GetHardcodedKey() => HardcodedKey;

        // VULNERABILIDAD: semilla aleatoria predecible
        public static string GenerateToken()
        {
            Random rnd = new Random(12345); // semilla fija = predecible
            return rnd.Next(100000, 999999).ToString();
        }
    }
}