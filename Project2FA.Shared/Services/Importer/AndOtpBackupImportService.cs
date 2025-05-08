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
using System;
using OtpNet;
using Project2FA.Core;
using Project2FA.Repository.Models.Enums;

// based on
// https://github.com/stratumauth/app/blob/48db7ed40cefa6e3d20b32172cb18da29da78503/Stratum.Core/src/Converter/AndOtpBackupConverter.cs

namespace Project2FA.Services.Importer
{
    public class AndOTPBackupImportService : IAndOTPBackupImportService
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
        public AndOTPBackupImportService(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }

        public async Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword)
        {
            string json;

            if (bytePassword is null)
            {
                json = content;
            }
            else
            {
                json = Decrypt(Encoding.UTF8.GetBytes(content), bytePassword);
            }

            List<AndOTPModel<string>> decryptedModel = SerializationService.Deserialize<List<AndOTPModel<string>>>(json);
            List<TwoFACodeModel> accountList = new List<TwoFACodeModel>();

            for (int i = 0; i < decryptedModel.Count; i++)
            {
                // check if the authentication methode is supported
                if (decryptedModel[i].Type == OTPType.totp.ToString().ToUpper() || decryptedModel[i].Type == OTPType.steam.ToString().ToUpper())
                {
                    OtpHashMode algorithm = decryptedModel[i].Algorithm switch
                    {
                        "SHA1" => OtpHashMode.Sha1,
                        "SHA256" => OtpHashMode.Sha256,
                        "SHA512" => OtpHashMode.Sha512,
                        _ => throw new ArgumentException($"Algorithm '{decryptedModel[i].Algorithm}' not supported")
                    };

                    var model = new TwoFACodeModel
                    {
                        Label = decryptedModel[i].Label,
                        TotpSize = decryptedModel[i].Digits,
                        Issuer = decryptedModel[i].Issuer,
                        Period = decryptedModel[i].Period,
                        HashMode = algorithm,
                        SecretByteArray = Encoding.UTF8.GetBytes(decryptedModel[i].Secret),
                        AccountIconName = DataService.Instance.GetIconForLabel(decryptedModel[i].Label.ToLower())
                    };
                    if (string.IsNullOrWhiteSpace(model.Issuer))
                    {
                        model.Issuer = decryptedModel[i].Label;
                    }
                    if (decryptedModel[i].Type == OTPType.steam.ToString().ToUpper())
                    {
                        model.OTPType = OTPType.steam.ToString();
                    }
                }
                else
                {
                    accountList.Add(new TwoFACodeModel
                    {
                        Label = decryptedModel[i].Label,
                        Issuer = decryptedModel[i].Issuer,
                        AccountIconName = DataService.Instance.GetIconForLabel(decryptedModel[i].Label.ToLower()),
                        IsEnabled = false,
                        IsChecked = false
                    });
                }
            }

            return (new List<TwoFACodeModel>(), false);
        }

        private KeyParameter DeriveKey(byte[] passwordBytes, byte[] salt, uint iterations)
        {
            var generator = new Pkcs5S2ParametersGenerator(new Sha1Digest());
            generator.Init(passwordBytes, salt, (int)iterations);
            return (KeyParameter)generator.GenerateDerivedParameters(BaseAlgorithm, KeyLength * 8);
        }

        private string Decrypt(byte[] data, byte[] passwordBytes)
        {
            var iterations = BinaryPrimitives.ReadUInt32BigEndian(data.Take(IterationsLength).ToArray());
            var salt = data.Skip(IterationsLength).Take(SaltLength).ToArray();
            var iv = data.Skip(IterationsLength + SaltLength).Take(IvLength).ToArray();
            var payload = data.Skip(IterationsLength + SaltLength + IvLength).ToArray();

            var key = DeriveKey(passwordBytes, salt, iterations);

            var keyParameter = new ParametersWithIV(key, iv);
            var cipher = CipherUtilities.GetCipher(AlgorithmDescription);
            cipher.Init(false, keyParameter);

            byte[] decrypted;

            decrypted = cipher.DoFinal(payload);
            return Encoding.UTF8.GetString(decrypted);

            //try
            //{
            //    decrypted = cipher.DoFinal(payload);
            //    return Encoding.UTF8.GetString(decrypted);
            //}
            //catch (InvalidCipherTextException e)
            //{
            //    //throw new BackupPasswordException("The password is incorrect", e);
            //}

            //return string.Empty;


        }
    }
}
