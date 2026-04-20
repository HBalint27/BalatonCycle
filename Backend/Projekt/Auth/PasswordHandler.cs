using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Projekt.Auth
{

    public static class PasswordHandler
    {
        public static string HashPassword(string password)
        {
            // Salt generálása
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = Environment.ProcessorCount,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] hash = argon2.GetBytes(32);

            // Salt + hash Base64-be kódolva
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] stored = Convert.FromBase64String(parts[1]);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = Environment.ProcessorCount,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] computed = argon2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(stored, computed);
        }

    }

}
