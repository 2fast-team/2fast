using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Project2FA.Repository.Models;
using UNOversal.Services.Logging;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto;
using Project2FA.Shared.Models;


#if WINDOWS_UWP
#else

using Project2FA.Uno;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Services.Importer
{
    public class BackupImporterService : IBackupImporterService
    {
        private ILoggingService LoggingService { get; }
        //private ISerializationService SerializationService { get; }
        private IAegisBackupImportService AegisBackupService { get; }
        private IAndOTPBackupImportService AndOTPBackupService { get; }
        private ITwoFASBackupImportService TwoFASBackupImportService { get; }

        public ITwofastBackupImportService TwofastBackupImportService { get; }
        public BackupImporterService(
            ILoggingService loggingService, 
            IAegisBackupImportService aegisBackupService,
            IAndOTPBackupImportService andOTPBackupService,
            ITwoFASBackupImportService twoFASBackupImportService,
            ITwofastBackupImportService twofastBackupImportService) 
        {
            LoggingService = loggingService;
            AegisBackupService = aegisBackupService;
            AndOTPBackupService = andOTPBackupService;
            TwoFASBackupImportService = twoFASBackupImportService;
            TwofastBackupImportService = twofastBackupImportService;
        }

        /// <summary>
        /// Gets the file content as string.
        /// </summary>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        private async Task<string> GetFileContent(StorageFile storageFile)
        {
            IRandomAccessStreamWithContentType randomStream = await storageFile.OpenReadAsync();
            using StreamReader streamReader = new StreamReader(randomStream.AsStreamForRead());
            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// Imports the Aegis backup file.
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(List<TwoFACodeModel> accountList,bool successful, Exception? exc)> ImportBackup(StorageFile storageFile, string password, BackupServiceEnum backupServiceEnum)
        {
            try
            {
                List<TwoFACodeModel> accountList = new List<TwoFACodeModel>();
                bool successful = false;
                switch (backupServiceEnum)
                {
                    case BackupServiceEnum.Aegis:
                        (accountList, successful) = await AegisBackupService.ImportBackup(await GetFileContent(storageFile), Encoding.UTF8.GetBytes(password));
                        break;
                    case BackupServiceEnum.AndOTP:
                        (accountList, successful) = await AndOTPBackupService.ImportBackup(await GetFileContent(storageFile), Encoding.UTF8.GetBytes(password));
                        break;
                    case BackupServiceEnum.TwoFAS:
                        (accountList, successful) = await TwoFASBackupImportService.ImportBackup(await GetFileContent(storageFile), Encoding.UTF8.GetBytes(password));
                        break;
                    case BackupServiceEnum.Twofast:
                        (accountList, successful) = await TwofastBackupImportService.ImportBackup(await GetFileContent(storageFile), Encoding.UTF8.GetBytes(password));
                        break;
                    default:
                        break;
                }

                if (successful)
                {
                    return (accountList, true, null);
                }
                else
                {
                    return (new List<TwoFACodeModel>(), false, null);
                }
            }
            catch (Exception exc)
            {
                if (exc is InvalidCipherTextException icte)
                {
                    // TODO wrong password
                }
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                return (new List<TwoFACodeModel>(), false, exc);
            }
        }
    }
}
