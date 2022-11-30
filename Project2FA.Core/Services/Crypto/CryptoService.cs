using System;
using System.Security.Cryptography;
using System.Text;

namespace Project2FA.Core.Services
{
    public static class CryptoService
    {
        public static byte[] CreateSHA512ByteArrayHash(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                return hash;
            }
        }

        public static byte[] CreateSHA512ByteArrayHash(byte[] inputByteArray)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                var hash = sha512.ComputeHash(inputByteArray);
                return hash;
            }
        }
        public static byte[] CreateByteArrayKey(string secret, int keyByteSize = 32, int iterations = 1000)
        {
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            var keyGenerator = new Rfc2898DeriveBytes(secret, salt, iterations);
            return keyGenerator.GetBytes(keyByteSize);
        }

        public static byte[] CreateByteArrayKey(byte[] secretByteArray, int keyByteSize = 32, int iterations = 1000)
        {
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            var keyGenerator = new Rfc2898DeriveBytes(secretByteArray, salt, iterations);
            return keyGenerator.GetBytes(keyByteSize);
        }

        public static string CreateStringHash(string input, bool argonHash = false)
        {
            // https://stackoverflow.com/questions/17292366/hashing-with-sha1-algorithm-in-c-sharp
            byte[] hash = CreateSHA512ByteArrayHash(input);
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();

        }

        public static string CreateStringHash(byte[] inputByteArray, bool argonHash = false)
        {
            // https://stackoverflow.com/questions/17292366/hashing-with-sha1-algorithm-in-c-sharp
            byte[] hash = CreateSHA512ByteArrayHash(inputByteArray);
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();

        }


    }
}
