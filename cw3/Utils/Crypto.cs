using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace cw3.Utils
{
    public static class Crypto
    {
        public static string CreateHash(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 100_000,
                numBytesRequested: 256 / 8
            );

            return Convert.ToBase64String(valueBytes);
        }

        public static string CreateSalt()
        {
            var randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}