using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Project2FA.Repository.Models;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOversal.Services.Serialization;

namespace Project2FA.Services.Importer
{
    public class AndOtpBackupImportService : IAndOTPBackupImportService
    {
        private const string BaseAlgorithm = "AES";
        private const string Mode = "GCM";
        private const string Padding = "NoPadding";
        private const string AlgorithmDescription = BaseAlgorithm + "/" + Mode + "/" + Padding;

        private const int IterationsLength = 4;
        private const int SaltLength = 12;
        private const int IvLength = 12;
        private const int KeyLength = 32;

        ISerializationService SerializationService { get; }
        public AndOtpBackupImportService(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }

        public Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword)
        {
            AndOTPModel<string> encryptedModel = SerializationService.Deserialize<AndOTPModel<string>>(content);
            return Task.FromResult((new List<TwoFACodeModel>(), false));
        }

        private static KeyParameter DeriveKey(string password, byte[] salt, uint iterations)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var generator = new Pkcs5S2ParametersGenerator(new Sha1Digest());
            generator.Init(passwordBytes, salt, (int)iterations);
            return (KeyParameter)generator.GenerateDerivedParameters(BaseAlgorithm, KeyLength * 8);
        }

        private static string Decrypt(byte[] data, string password)
        {
            var iterations = BinaryPrimitives.ReadUInt32BigEndian(data.Take(IterationsLength).ToArray());
            var salt = data.Skip(IterationsLength).Take(SaltLength).ToArray();
            var iv = data.Skip(IterationsLength + SaltLength).Take(IvLength).ToArray();
            var payload = data.Skip(IterationsLength + SaltLength + IvLength).ToArray();

            var key = DeriveKey(password, salt, iterations);

            var keyParameter = new ParametersWithIV(key, iv);
            var cipher = CipherUtilities.GetCipher(AlgorithmDescription);
            cipher.Init(false, keyParameter);

            byte[] decrypted;

            try
            {
                decrypted = cipher.DoFinal(payload);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (InvalidCipherTextException e)
            {
                //throw new BackupPasswordException("The password is incorrect", e);
            }

            return string.Empty;


        }
    }
}
