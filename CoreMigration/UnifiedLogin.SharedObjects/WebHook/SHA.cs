using System.Security.Cryptography;
using System.Text;

namespace UnifiedLogin.SharedObjects.WebHook
{
    /// <summary>
    /// Used to validate the webhook request
    /// </summary>
    public static class SHA
    {
        public static string GenerateHMACSHA256String(string key, string inputString)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] textBytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hashBytes;
            using (var hash = new HMACSHA256(keyBytes))
            {
                hashBytes = hash.ComputeHash(textBytes);
            }
            return GetStringFromHash(hashBytes);
        }

        public static string GenerateSHA256String(string inputString)
        {
            var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        public static string GenerateSHA512String(string inputString)
        {
            var sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            var result = new StringBuilder();
            foreach (byte t in hash)
            {
                result.Append(t.ToString("X2"));
            }
            return result.ToString().ToLower();
        }
    }
}
