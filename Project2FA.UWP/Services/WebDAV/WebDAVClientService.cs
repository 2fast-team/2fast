using System;
using Prism.Ioc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.Secrets;
using WebDAVClient;

namespace Project2FA.UWP.Services.WebDAV
{
    public class WebDAVClientService : IWebDAVClientService
    {
        /// <summary>
        /// Private singleton field.
        /// </summary>
        [ThreadStatic]
        private static WebDAVClientService _instance;

        ISecretService _secretService { get; }

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static WebDAVClientService Instance => _instance ?? (_instance = new WebDAVClientService());
        private Client _client;

        public WebDAVClientService()
        {
            _secretService = App.Current.Container.Resolve<ISecretService>();
        }
        public async Task<Client> GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            if (!string.IsNullOrEmpty(_secretService.Helper.ReadSecret("Project2FA", "WDServerAddress")) &&
                !string.IsNullOrEmpty(_secretService.Helper.ReadSecret("Project2FA", "WDUsername")))
            {

                string username, serveraddress, password;
                username = _secretService.Helper.ReadSecret("Project2FA", "WDUsername");
                password = _secretService.Helper.ReadSecret("Project2FA", "WDPassword");
                serveraddress = _secretService.Helper.ReadSecret("Project2FA", "WDServerAddress");
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

                _client = new Client(serveraddress, username, password);
            }



            return _client;
        }

        public void Reset()
        {
            _client = null;
        }
    }
}
