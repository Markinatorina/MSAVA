using System.Security.Cryptography;
using System.Text;

namespace M_SAVA_BLL.Utils
{
    public static class PasswordUtils
    {
        private const int SaltSize = 32;
        private const int HashIterations = 100000;
        private const int HashByteSize = 32;

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        public static byte[] HashPassword(string password, byte[] salt)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, HashIterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(HashByteSize);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            byte[] hash = HashPassword(password, storedSalt);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
