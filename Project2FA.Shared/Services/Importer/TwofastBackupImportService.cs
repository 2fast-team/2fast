using Project2FA.Core.Services.Crypto;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UNOversal.Services.Serialization;

namespace Project2FA.Services.Importer
{
    public class TwofastBackupImportService : ITwofastBackupImportService
    {
        public ISerializationService SerializationService { get; }

        public ISerializationCryptoService SerializationCryptoService { get; }
        public TwofastBackupImportService(ISerializationService serializationService, ISerializationCryptoService serializationCryptoService)
        {
            SerializationService = serializationService;
            SerializationCryptoService = serializationCryptoService;
        }
        public Task<(List<TwoFACodeModel> accountList, bool successful)> ImportBackup(string content, byte[] bytePassword)
        {
            if (bytePassword is null || bytePassword.Length == 0)
            {
                //throw new ArgumentException("Password required but not provided");
                return Task.FromResult((new List<TwoFACodeModel>(), false));
            }
            // read the iv for AES
            DatafileModel datafile = SerializationService.Deserialize<DatafileModel>(content);
            Aes algorithm = Aes.Create();
            algorithm.IV = datafile.IV;

            if (datafile.Version == 1 || datafile.Version == 0)
            {
                algorithm.Key = CryptoService.CreateByteArrayKeyV1(bytePassword);
            }
            else
            {
                algorithm.Key = CryptoService.CreateByteArrayKeyV2(bytePassword);
            }

            // deserialize and decrypt the datafile
            datafile = SerializationCryptoService.DeserializeDecrypt<DatafileModel>(algorithm.Key, algorithm.IV, content, datafile.Version);

            return Task.FromResult((datafile.Collection.ToList(), true));
        }
    }
}
