using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UNOversal.Services.Serialization;

namespace Project2FA.Services.Importer
{
    public class TwoFASBackupImportService : ITwoFASBackupImportService
    {
        private const string BaseAlgorithm = "AES";
        private const string Mode = "GCM";
        private const string Padding = "NoPadding";
        private const string AlgorithmDescription = BaseAlgorithm + "/" + Mode + "/" + Padding;

        private const int Iterations = 10000;
        private const int KeyLength = 32;

        ISerializationService SerializationService { get; }
        public TwoFASBackupImportService(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }

        public Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword)
        {
            var backup = SerializationService.Deserialize<TwoFASBackup>(content);

            if (backup.ServicesEncrypted != null)
            {
                if (bytePassword == null)
                {
                    //throw new ArgumentException("Password required but not provided");
                    return Task.FromResult((new List<TwoFACodeModel>(), false));
                }

                var decryptedContent = DecryptServices(backup.ServicesEncrypted, bytePassword);
                if (!string.IsNullOrWhiteSpace(decryptedContent))
                {
                    backup.Services = SerializationService.Deserialize<List<TwoFASServiceModel>>(decryptedContent);

                    for (int i = 0; i < backup.Services.Count; i++)
                    {

                    }
                    //return Task.FromResult((accountList, true));
                }
                else
                {
                    return Task.FromResult((new List<TwoFACodeModel>(), false));
                }
            }
            else
            {

            }
            return Task.FromResult((new List<TwoFACodeModel>(), false));
        }

        private string DecryptServices(string payload, byte[] bytePassword)
        {
            var parts = payload.Split(":", 3);

            if (parts.Length < 3)
            {
                throw new ArgumentException("Invalid payload format");
            }

            var encryptedData = Convert.FromBase64String(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var iv = Convert.FromBase64String(parts[2]);

            var key = DeriveKey(bytePassword, salt);
            var keyParameter = new ParametersWithIV(key, iv);
            var cipher = CipherUtilities.GetCipher(AlgorithmDescription);
            cipher.Init(false, keyParameter);

            byte[] decryptedBytes;

            try
            {
                decryptedBytes = cipher.DoFinal(encryptedData);
                var decryptedJson = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedJson;
            }
            catch (InvalidCipherTextException e)
            {
                //throw new BackupPasswordException("The password is incorrect", e);
                return string.Empty;
            }
        }

        private KeyParameter DeriveKey(byte[] bytePassword, byte[] salt)
        {
            var generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            generator.Init(bytePassword, salt, Iterations);
            return (KeyParameter)generator.GenerateDerivedParameters(BaseAlgorithm, KeyLength * 8);
        }
    }
}
