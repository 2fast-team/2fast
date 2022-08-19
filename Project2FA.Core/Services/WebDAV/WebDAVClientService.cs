using System;
using System.Threading.Tasks;
using WebDAVClient;

namespace Project2FA.Core.Services.WebDAV
{
    public class WebDAVClientService
    {
        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static WebDAVClientService Instance { get; } = new WebDAVClientService();
        private WebDAVClient.Client _client;

        public WebDAVClientService()
        {

        }
        public Client GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            //if (!string.IsNullOrEmpty(await SecureStorage.GetAsync("WDServerAddress")) &&
            //    !string.IsNullOrEmpty(await SecureStorage.GetAsync("WDUsername")))
            //{

            //    string username, serveraddress, password;
            //    username = await SecureStorage.GetAsync("WDUsername");
            //    password = await SecureStorage.GetAsync("WDPassword");
            //    serveraddress = await SecureStorage.GetAsync("WDServerAddress");
            //    try
            //    {

            //    }
            //    catch
            //    {
            //        //await ShowServerAddressNotFoundMessage(credential.Resource);
            //        //return null;
            //    }

            //    _client = new WebDAVClient.Client(serveraddress, username, password);
            //}  

            return null;
        }

        public void Reset()
        {
            _client = null;
        }
    }
}
