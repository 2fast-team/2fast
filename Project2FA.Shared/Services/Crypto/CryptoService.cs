using Project2FA.Repository.Models;
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

        /// <summary>
        /// Creates the password key for the specified data file using the version-specific key derivation algorithm.
        /// </summary>
        /// <param name="datafile">The data file metadata, including the version and salt used to derive the password key.</param>
        /// <param name="passwordByte">The password bytes used to derive the key.</param>
        /// <returns>The derived password key as a byte array.</returns>
        public static byte[] CreatePasswordKey(DatafileModel datafile, byte[] passwordByte)
        {
            switch (datafile.Version)
            {
                case 0:
                case 1:
                default:
                    return CreateByteArrayKeyV1(passwordByte);
                case 2:
                    return CreateByteArrayKeyV2(passwordByte);
                case 3:
                    return CreateByteArrayKeyV3(passwordByte, datafile.Salt);
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

        /// <summary>
        /// Derives a key using PBKDF2-SHA256 with a per-file random salt (16 bytes).
        /// Use for Version 3+ datafiles. The salt must be stored in the DatafileModel.
        /// </summary>
        public static byte[] CreateByteArrayKeyV3(byte[] secretByteArray, byte[] salt, int keyByteSize = 32, int iterations = 25000)
        {
            if (salt == null || salt.Length < 16)
                throw new ArgumentException("Salt must be at least 16 bytes.", nameof(salt));
#if NET9_0_OR_GREATER
            return Rfc2898DeriveBytes.Pbkdf2(secretByteArray, salt, iterations, HashAlgorithmName.SHA256, keyByteSize);
#else
            var keyGenerator = new Rfc2898DeriveBytes(secretByteArray, salt, iterations, HashAlgorithmName.SHA256);
            return keyGenerator.GetBytes(keyByteSize);
#endif
        }

        /// <summary>
        /// Generates a cryptographically random 16-byte salt for use with V3 datafiles.
        /// </summary>
        public static byte[] GenerateRandomSalt(int byteSize = 16)
        {
            byte[] salt = new byte[byteSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
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
