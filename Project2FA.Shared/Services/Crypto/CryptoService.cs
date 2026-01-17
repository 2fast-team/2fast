using System;
using System.Security.Cryptography;
using System.Text;

namespace Project2FA.Core.Services.Crypto
{
    public static class CryptoService
    {
        private static byte[]? _key;
        private static byte[]? _iv;

        public static void Initialize(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes (256 bits) long.", nameof(key));
            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes (128 bits) long.", nameof(iv));
            _key = key;
            _iv = iv;
        }
        public static bool IsInitialized()
        {
            return _key != null && _iv != null;
        }

        public static void Clear()
        {
            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length);
                _key = null;
            }
            if (_iv != null)
            {
                Array.Clear(_iv, 0, _iv.Length);
                _iv = null;
            }
        }
        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);

            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Encrypt(byte[] bytes)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor();

            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);

            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        public static byte[] Decrypt(byte[] decryptedBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor();

            var decrypted = decryptor.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
            return decrypted;
        }

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

        public static byte[] CreateByteArrayKeyV1(byte[] secretByteArray, int keyByteSize = 32, int iterations = 1000)
        {
#if NET9_0_OR_GREATER
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            return Rfc2898DeriveBytes.Pbkdf2(secretByteArray, salt, iterations, HashAlgorithmName.SHA1, keyByteSize);
#else
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            var keyGenerator = new Rfc2898DeriveBytes(secretByteArray, salt, iterations, HashAlgorithmName.SHA1);
            return keyGenerator.GetBytes(keyByteSize);
#endif
        }

        public static byte[] CreateByteArrayKeyV2(byte[] secretByteArray, int keyByteSize = 32, int iterations = 25000)
        {
#if NET9_0_OR_GREATER
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            return Rfc2898DeriveBytes.Pbkdf2(secretByteArray, salt, iterations, HashAlgorithmName.SHA256, keyByteSize);
#else
            byte[] salt = new byte[8] { 14, 223, 35, 197, 93, 242, 239, 8 };
            var keyGenerator = new Rfc2898DeriveBytes(secretByteArray, salt, iterations, HashAlgorithmName.SHA256);
            return keyGenerator.GetBytes(keyByteSize);
#endif
        }

        public static string CreateStringHash(string input)
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

        public static string CreateStringHash(byte[] inputByteArray)
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
