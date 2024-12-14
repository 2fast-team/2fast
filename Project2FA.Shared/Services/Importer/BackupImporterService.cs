using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UNOversal.Services.File;
using Windows.Storage;
using Windows.Storage.Streams;
using UNOversal.Ioc;
using UNOversal.Services.Serialization;
using Project2FA.Repository.Models;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Encoders;
using UNOversal.Services.Logging;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else

using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Services.Importer
{
    public class BackupImporterService : IBackupImporterService
    {
        private ILoggingService LoggingService { get; }
        public BackupImporterService(ILoggingService loggingService) 
        {
            LoggingService = loggingService;
        }
        public async Task ImportAegisBackup(StorageFile storageFile, string password)
        {
            try
            {
                IRandomAccessStreamWithContentType randomStream = await storageFile.OpenReadAsync();
                using StreamReader streamReader = new StreamReader(randomStream.AsStreamForRead());

                password = "test";
                var serializationService = App.Current.Container.Resolve<ISerializationService>();
                AegisModel<string> encryptedModel = serializationService.Deserialize<AegisModel<string>>(await streamReader.ReadToEndAsync());

                // search for password slot
                if (encryptedModel.Header.Slots != null && encryptedModel.Header.Slots.Count > 0)
                {
                    var slot = encryptedModel.Header.Slots.Where(x => x.Type == AegisSlotType.Password).FirstOrDefault();
                    if (slot != null)
                    {
                        byte[] saltBytes = HexStringToByteArray(slot.Salt);

                        // SCrypt parameters
                        int n = slot.N;  // Cost factor
                        int r = slot.R;  // Block size
                        int p = slot.P;  // Parallelization factor

                        // Derive the key
                        int keySize = 32; // Key size in bytes
                        byte[] derivedKey = SCrypt.Generate(Encoding.UTF8.GetBytes(password), saltBytes, n, r, p, keySize);

                        // get data and initialisation vector
                        byte[] ciphertextTagBytes = Convert.FromBase64String(encryptedModel.Database);
                        byte[] ivBytes = Hex.Decode(encryptedModel.Header.Params.Nonce);
                        byte[] macBytes = Hex.Decode(encryptedModel.Header.Params.Tag);
                        //byte[] plaintextBytes = new byte[ciphertextTagBytes.Length - macBytes.Length];

                        // decrypt

                    }
                    else
                    {
                        // TODO return error
                    }
                }
                else
                {
                    // TODO return error
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                // TODO return error
            }
        }

        private static string DecryptWithGCM(string ciphertextTag, string keyString, string nonceString)
        {
            var tagLength = 16;
            var key = Convert.FromBase64String(keyString);
            var nonce = Convert.FromBase64String(nonceString);

            var ciphertextTagBytes = Convert.FromBase64String(ciphertextTag);
            var plaintextBytes = new byte[ciphertextTagBytes.Length - tagLength];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, nonce);
            cipher.Init(false, parameters);

            var offset = cipher.ProcessBytes(ciphertextTagBytes, 0, ciphertextTagBytes.Length, plaintextBytes, 0);
            cipher.DoFinal(plaintextBytes, offset); // authenticate data via tag

            return Encoding.UTF8.GetString(plaintextBytes);
        }


        private byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
