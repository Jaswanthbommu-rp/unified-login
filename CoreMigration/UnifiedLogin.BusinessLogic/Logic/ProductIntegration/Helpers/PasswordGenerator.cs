using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    public class PasswordGenerator
    {
        public static string GeneratePassword(int length, int specials)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const string specialChars = "!@$?_-.";

            Span<char> buffer = stackalloc char[length];
            for (int i = 0; i < length - specials; i++)
                buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            for (int i = length - specials; i < length; i++)
                buffer[i] = specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)];
            // optional shuffle
            for (int i = buffer.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
            return new string(buffer);
        }
    }
}
