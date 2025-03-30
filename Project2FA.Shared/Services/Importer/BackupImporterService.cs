using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using UNOversal.Services.Serialization;
using Project2FA.Repository.Models;
using UNOversal.Services.Logging;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else

using Project2FA.Uno;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

// based on
// https://github.com/stratumauth/app/blob/master/Stratum.Core/src/Converter/

namespace Project2FA.Services.Importer
{
    public class BackupImporterService : IBackupImporterService
    {
        private ILoggingService LoggingService { get; }
        //private ISerializationService SerializationService { get; }
        private IAegisBackupService AegisBackupService { get; }
        public BackupImporterService(
            ILoggingService loggingService, 
            //ISerializationService serializationService,
            IAegisBackupService aegisBackupService) 
        {
            LoggingService = loggingService;
            //SerializationService = serializationService;
            AegisBackupService = aegisBackupService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(List<TwoFACodeModel> accountList,bool successful)> ImportAegisBackup(StorageFile storageFile, string password)
        {
            try
            {
                IRandomAccessStreamWithContentType randomStream = await storageFile.OpenReadAsync();
                using StreamReader streamReader = new StreamReader(randomStream.AsStreamForRead());
                (var accountList, bool successful) = await AegisBackupService.ImportBackup(await streamReader.ReadToEndAsync(), Encoding.UTF8.GetBytes(password));

                if (successful)
                {
                    return (accountList, true);
                }
                else
                {
                    return (null, false);
                }
            }
            catch (Exception exc)
            {
                if (exc is InvalidCipherTextException icte)
                {
                    // TODO wrong password
                }
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                return (null, false);
            }
        }
    }
}
