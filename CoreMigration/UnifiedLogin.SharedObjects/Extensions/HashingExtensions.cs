using System;
using System.Security.Cryptography;
using System.Text;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Extensions
{
    public static class HashingExtensions
    {
        private const int PasswordHashLength = 128;
        private const int IterationCount = 10000;
        private const int SaltByteSize = 128;

        public static string Sha256(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }

        public static PasswordDetail PasswordHash(this string password)
        {
            var passwordDetail = new PasswordDetail();
            var salt = GetSalt();
            passwordDetail.PasswordHash = PasswordHashBySalt(password, salt);
            passwordDetail.PasswordSalt = Convert.ToBase64String(salt);
            return passwordDetail;
        }
        public static string PasswordHashBySalt(this string password, byte[] salt)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = IterationCount };
            return Convert.ToBase64String(pbkdf2.GetBytes(PasswordHashLength));
        }

        private static byte[] GetSalt()
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[SaltByteSize];
                cryptoProvider.GetBytes(salt);
                return salt;
            }
        }
    }
}