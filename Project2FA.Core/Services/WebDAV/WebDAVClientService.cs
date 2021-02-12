using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebDAVClient;
using Xamarin.Essentials;

namespace Project2FA.Core.Services.WebDAV
{
    public class WebDAVClientService
    {
        /// <summary>
        /// Private singleton field.
        /// </summary>
        [ThreadStatic]
        private static WebDAVClientService _instance;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static WebDAVClientService Instance => _instance ?? (_instance = new WebDAVClientService());
        private WebDAVClient.Client _client;

        public WebDAVClientService()
        {

        }
        public async Task<Client> GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            if (!string.IsNullOrEmpty(await SecureStorage.GetAsync("WDServerAddress")) &&
                !string.IsNullOrEmpty(await SecureStorage.GetAsync("WDUsername")))
            {

                string username, serveraddress, password;
                username = await SecureStorage.GetAsync("WDUsername");
                password = await SecureStorage.GetAsync("WDPassword");
                serveraddress = await SecureStorage.GetAsync("WDServerAddress");
                try
                {
                    //var response = await OcsClient.GetServerStatusAsync(Configuration.ServerUrl);
                    //if (response == null)
                    //{
                        //var checkServerResponse = await NextcloudClient.NextcloudClient.GetServerStatus(credential.Resource, true);
                        //if (checkServerResponse != null)
                        //{
                        //    var dialogResult = await ShowServerCertificateErrorMessage(credential.Resource);
                        //    switch (dialogResult)
                        //    {
                        //        case ContentDialogResult.Primary:
                        //            try
                        //            {
                        //                SettingsService.Default.Value.LocalSettings.IgnoreServerCertificateErrors = true;
                        //                //response = await NextcloudClient.NextcloudClient.GetServerStatus(credential.Resource, SettingsService.Default.Value.LocalSettings.IgnoreServerCertificateErrors);
                        //            }
                        //            catch (System.Exception)
                        //            {
                        //                await ShowServerAddressNotFoundMessage(credential.Resource);
                        //                return null;
                        //            }
                        //            break;
                        //        case ContentDialogResult.None:
                        //        case ContentDialogResult.Secondary:
                        //            await ShowServerAddressNotFoundMessage(credential.Resource);
                        //            return null;
                        //        default:
                        //            break;
                        //    }
                        //}
                        //else
                        //{
                        //    await ShowServerAddressNotFoundMessage(credential.Resource);
                        //    return null;
                        //}
                    //    await ShowServerAddressNotFoundMessage(credential.Resource);
                    //    return null;
                    //}
                }
                catch
                {
                    //await ShowServerAddressNotFoundMessage(credential.Resource);
                    //return null;
                }

                _client = new WebDAVClient.Client(serveraddress, username, password);
            }

            

            return _client;
        }

        public void Reset()
        {
            _client = null;
        }
    }
}
