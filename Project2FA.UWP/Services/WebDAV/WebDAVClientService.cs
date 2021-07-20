using System;
using Prism.Ioc;
using Template10.Services.Secrets;
using WebDAVClient;
using Project2FA.Core;

namespace Project2FA.UWP.Services.WebDAV
{
    public class WebDAVClientService : IDisposable
    {
        ISecretService SecretService { get; }

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static WebDAVClientService Instance { get; } = new WebDAVClientService();
        private Client _client;

        public WebDAVClientService()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
        }
        public Client GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            if (!string.IsNullOrEmpty(SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress")) &&
                !string.IsNullOrEmpty(SecretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername")))
            {

                string username, serveraddress, password;
                username = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername");
                password = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDPassword");
                serveraddress = SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress");
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

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
