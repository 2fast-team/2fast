
using Project2FA.Core;
using Project2FA.Core.Services.JSON;
using Project2FA.Services.Enums;
using System;
using System.Threading.Tasks;
using UNOversal.Services.File;
using Windows.Storage;

namespace Project2FA.Services.Logging
{
    /// <summary>
    /// Service for logging of exception and custom messages
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private string _timeStemp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - ";
        
        private IFileService FileService { get; }

        public LoggingService(IFileService fileService)
        {
            FileService = fileService;
        }

        /// <summary>
        /// Log a custom message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Log(string message)
        {
            if (SettingsService.Instance.LoggingSetting == LoggingPreferEnum.Full)
            {
                if(await FileService.FileExistsAsync(Constants.LogName, ApplicationData.Current.LocalFolder))
                {
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.LogName);
                    await FileIO.AppendTextAsync(file, _timeStemp + message);
                }
                else
                {
                    var file =  await ApplicationData.Current.LocalFolder.CreateFileAsync(Constants.LogName);
                    await FileIO.AppendTextAsync(file, _timeStemp + message);
                }
            }
        }

        /// <summary>
        /// Log a exception
        /// </summary>
        /// <param name="exc"></param>
        /// <returns></returns>
        public async Task LogException(Exception exc)
        {
            if (SettingsService.Instance.LoggingSetting == LoggingPreferEnum.Simple ||
                SettingsService.Instance.LoggingSetting == LoggingPreferEnum.Full)
            {
                if (await FileService.FileExistsAsync(Constants.LogName, ApplicationData.Current.LocalFolder))
                {
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.LogName);
                    await WriteExceptionLog(exc, file);

                }
                else
                {
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(Constants.LogName);
                    await WriteExceptionLog(exc, file);
                }
            }
        }

        /// <summary>
        /// Write the exception to a file
        /// </summary>
        /// <param name="exc"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task WriteExceptionLog(Exception exc, StorageFile file)
        {
#if WINDOWS_UWP
            await FileIO.AppendTextAsync(file, _timeStemp +  "uptime " + Microsoft.Toolkit.Uwp.Helpers.SystemInformation.Instance.AppUptime + "\n");
#endif
            await FileIO.AppendTextAsync(file, _timeStemp + exc.Source + " - " + exc.Message + "\n");
            await FileIO.AppendTextAsync(file, "Log - " + exc.ToString() + "\n");
        }
    }
}
