using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Xml.Linq;
using WebDAVClient.Exceptions;
using WebDAVClient.Types;
using DecaTec.WebDav;
using DecaTec.WebDav.WebDavArtifacts;
using WebDAVClient.WebDav;
using WebDAVClient.Extensions;
using DecaTec.WebDav.Tools;
using System.Net.Http;
using System.Net;

//fork from https://github.com/nextcloud/windows-universal

namespace WebDAVClient
{
    /// <summary>
    /// Nextcloud OCS and DAV access client
    /// </summary>
    public class Client : IDisposable
    {
        #region PRIVATE PROPERTIES

        /// <summary>
        /// WebDavNet instance.
        /// </summary>
        private readonly WebDavSession _dav;

        /// <summary>
        /// Server Base URL.
        /// </summary>
        private readonly string _url;

        /// <summary>
        /// The client
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// The HTTP handler
        /// </summary>
        private readonly HttpClientHandler _httpClientHandler;

        /// <summary>
        /// Nextcloud WebDAV access path.
        /// </summary>
        private const string Davpath = "remote.php/webdav";

        /// <summary>
        /// Nextcloud OCS API access path.
        /// </summary>
        private const string Ocspath = "ocs/v1.php";

        /// <summary>
        /// OCS Share API path.
        /// </summary>
        private const string OcsServiceShare = "apps/files_sharing/api/v1";

        private const string OcsServiceData = "privatedata";

        /// <summary>
        /// OCS Provisioning API path.
        /// </summary>
        private const string OcsServiceCloud = "cloud";

        private readonly PropFind _webDAVPropFind;

        private readonly Version HttpVersion = new Version(2, 0);

        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        private static JsonSerializerOptions _jsonSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="url">WebDAV instance URL.</param>
        /// <param name="userId">User identifier.</param>
        /// <param name="password">Password.</param>
        public Client(string url, string userId, string password, bool ignoreServerCertificateErrors = false)
            : this(url, new NetworkCredential(userId, password), ignoreServerCertificateErrors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDAVClient"/> class.
        /// </summary>
        /// <param name="url">Nextcloud instance URL.</param>
        /// <param name="httpClientHandler">The HTTP base protocol filter.</param>
        public Client(string url, NetworkCredential passwordCredential, bool ignoreServerCertificateErrors = false)
        {
            if (url == null)
            {
                return;
            }

            // In case URL has a trailing slash remove it
            if (url.EndsWith("/", StringComparison.InvariantCulture))
            {
                url = url.TrimEnd('/');
            }

            // Create PropFind which contains all NC specific properties.
            PropFind propFind = PropFind.CreatePropFindWithEmptyPropertiesAll();
            Prop prop = (Prop)propFind.Item;

            XNamespace nsOc = "http://owncloud.org/ns";

            List<XElement> xElementList = new List<XElement>();
            XElement xElement = new XElement(nsOc + WebDAVPropNameConstants.Checksums);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.CommentsCount);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.CommentsHref);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.CommentsUnread);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.Favorite);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.FileId);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.HasPreview);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.Id);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.OwnerDisplayName);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.OwnerId);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.ShareTypes);
            xElementList.Add(xElement);
            xElement = new XElement(nsOc + WebDAVPropNameConstants.Size);
            xElementList.Add(xElement);

            prop.AdditionalProperties = xElementList.ToArray();
            _webDAVPropFind = propFind;

            _url = url;

            _httpClientHandler = new HttpClientHandler()
            {
                PreAuthenticate = true,
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                // Specify the user credentials.
                Credentials = passwordCredential
            };

            _httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (ignoreServerCertificateErrors)
                {
                    IgnoreServerCertificateErrors = true;
                    // Specify which certificate errors should be ignored.
                    //TODO check if other errors should be ignored
                    return errors == System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable;
                }
                else
                {
                    switch (errors)
                    {
                        case System.Net.Security.SslPolicyErrors.None:
                            return true;
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors:
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch:
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable:
                        default:
                            return false;
                    }
                }
            };

            _client = new HttpClient(_httpClientHandler);
            _client.DefaultRequestHeaders.Add("Pragma", "no-cache");

            string encoded =
                Convert.ToBase64String(
                    Encoding.GetEncoding("ISO-8859-1").GetBytes(
                        passwordCredential.UserName + ":" +
                        passwordCredential.Password
                    ));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);

            _dav = new WebDavSession(_url, _httpClientHandler, HttpVersion)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        #endregion

        #region Settings

        /// <summary>
        /// Gets or sets a value indicating whether to ignore server certificate errors.
        /// Be careful, setting this to <c>true</c> will allow MITM attacks!
        /// </summary>
        /// <value>
        /// <c>true</c> if server certificate errors are ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreServerCertificateErrors
        {
            get;
            set;
        }

        #endregion

        #region DAV

        /// <summary>
        /// List the specified remote path.
        /// </summary>
        /// <param name="path">remote Path.</param>
        /// <returns>List of Resources.</returns>
        public async Task<List<ResourceInfoModel>> List(string path)
        {
            List<ResourceInfoModel> resources = new List<ResourceInfoModel>();
            IList<WebDavSessionItem> result = await _dav.ListAsync(GetDavUri(path), _webDAVPropFind);

            Uri baseUri = new Uri(_url);
            baseUri = new Uri(baseUri, baseUri.AbsolutePath + (baseUri.AbsolutePath.EndsWith("/") ? "" : "/") + Davpath);

            foreach (WebDavSessionItem item in result)
            {
                ResourceInfoModel res = item.ToResourceInfo(baseUri);

                if (!res.IsDirectory)
                {
                    // if resource not a directory, remove the file name from remote path.
                    res.Path = res.Path.Replace("/" + res.Name, "");
                }

                resources.Add(res);
            }

            return resources;
        }

        /// <summary>
        /// Gets the resource info for the remote path.
        /// </summary>
        /// <returns>The resource info.</returns>
        /// <param name="path">remote Path.</param>
        /// <param name="name">name of resource to get</param>
        public async Task<ResourceInfoModel> GetResourceInfoAsync(string path, string name)
        {
            Uri baseUri = new Uri(_url);
            baseUri = new Uri(baseUri, baseUri.AbsolutePath + (baseUri.AbsolutePath.EndsWith("/") ? "" : "/") + Davpath);

            IList<WebDavSessionItem> result = await _dav.ListAsync(GetDavUri(path), _webDAVPropFind);

            if (!result.Any())
            {
                return null;
            }
            foreach (WebDavSessionItem item in result)
            {
                if (item.Name.Equals(name, StringComparison.Ordinal))
                {
                    ResourceInfoModel res = item.ToResourceInfo(baseUri);

                    if (!res.IsDirectory)
                    {
                        // if resource not a directory, remove the file name from remote path.
                        res.Path = res.Path.Replace("/" + res.Name, "");
                    }
                    return res;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds remote outgoing shares.
        /// </summary>
        /// <returns>List of shares.</returns>
        public async Task<List<ResourceInfoModel>> GetSharesView(string viewname)
        {
            Tuple<string, string> param = new Tuple<string, string>("shared_with_me", "false");
            if (viewname == "sharesIn")
            {
                param = new Tuple<string, string>("shared_with_me", "true");
            }

            List<Share> shares = await GetShares(param);

            List<ResourceInfoModel> sharesList = new List<ResourceInfoModel>();

            foreach (Share item in shares)
            {
                if (viewname == "sharesLink")
                {
                    string type = item.GetType().ToString();
                    if (type != "NextcloudClient.Types.PublicShare")
                    {
                        continue;
                    }
                    ResourceInfoModel itemShare = await GetResourceInfoByPath(item.Path);
                    sharesList.Add(itemShare);
                }
                else
                {
                    ResourceInfoModel itemShare = await GetResourceInfoByPath(item.Path);
                    sharesList.Add(itemShare);
                }
            }

            return sharesList;
        }

        /// <summary>
        /// Finds user favorites.
        /// </summary>
        /// <returns>List of favorites.</returns>
        public async Task<List<ResourceInfoModel>> GetFavorites()
        {
            UrlBuilder url = new UrlBuilder(_url + "/remote.php/webdav");

            // See: https://docs.nextcloud.com/server/12/developer_manual/client_apis/WebDAV/index.html#listing-favorites
            // Also, for Props see: https://docs.nextcloud.com/server/12/developer_manual/client_apis/WebDAV/index.html
            const string content = "<?xml version=\"1.0\"?>"
                + "<oc:filter-files  xmlns:d=\"DAV:\" xmlns:oc=\"http://owncloud.org/ns\" xmlns:nc=\"http://nextcloud.org/ns\">"
                + "<d:prop>"
                + "<oc:favorite />"
                + "</d:prop>"
                + "<oc:filter-rules>"
                + "<oc:favorite>1</oc:favorite>"
                + "</oc:filter-rules>"
                + "</oc:filter-files>";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("REPORT"), url.ToUri())
            {
                Content = new StringContent(content, UnicodeEncoding.UTF8, "application/xml")
            };

            HttpResponseMessage response = await _client.SendAsync(request);

            string contentString = await response.Content.ReadAsStringAsync();
            Multistatus multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentString(contentString);
            List<ResourceInfoModel> favoritesList = new List<ResourceInfoModel>();

            if (multistatus.Response != null)
            {
                foreach (Response msResponse in multistatus.Response)
                {
                    foreach (object item in msResponse.Items)
                    {
                        string href = item as string;

                        if (string.IsNullOrEmpty(href) || !href.Contains(Davpath))
                        {
                            continue;
                        }

                        href = href.TrimEnd('/');
                        href = href.Replace("/remote.php/webdav", "");
                        ResourceInfoModel itemFav = await GetResourceInfoByPath(href);

                        favoritesList.Add(itemFav);
                    }
                }
            }

            return favoritesList;
        }

        /// <summary>
        /// Finds resource info for item by searching its parent.
        /// </summary>
        /// <returns>Resource Info if given item.</returns>
        /// <param name="path">Path to the Item.</param>
        private async Task<ResourceInfoModel> GetResourceInfoByPath(string path)
        {
            path = Uri.UnescapeDataString(path);
            string targetPath = "/" + path.Split('/')[path.Split('/').Length - 1];
            string parentPath = path.Replace(targetPath, "/");
            string itemName = targetPath.Replace("/", "");

            List<ResourceInfoModel> parentResource = await List(parentPath);
            ResourceInfoModel itemResource = new ResourceInfoModel();

            foreach (ResourceInfoModel item in parentResource)
            {
                if (item.Name == itemName)
                {
                    itemResource = item;
                }
            }

            return itemResource;
        }

        /// <summary>
        /// Download the specified file.
        /// </summary>
        /// <param name="path">File remote Path.</param>
        /// <param name="localStream"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns>File contents.</returns>
        public Task<bool> Download(string path, Stream localStream, IProgress<WebDavProgress> progress, CancellationToken cancellationToken)
        {
            return _dav.DownloadFileWithProgressAsync(GetDavUri(path, true), localStream, progress, cancellationToken);
        }

        public async Task<Stream> GetImage(ResourceInfoModel file)
        {
            if (!file.ContentType.StartsWith(@"image/", StringComparison.InvariantCulture))
            {
                return null;
            }

            Uri uri = new Uri(GetDavUri(file.Path, true) + "/" + Uri.EscapeDataString(file.Name));
            _client.DefaultRequestHeaders.Add("Cookie", "nc_sameSiteCookielax=true;nc_sameSiteCookiestrict=true");

            HttpResponseMessage response = await _client.GetAsync(uri);

            _client.DefaultRequestHeaders.Remove("Cookie");

            if (response != null)
            {
                return await response.Content.ReadAsStreamAsync();
            }

            // TODO: Errorhandling
            Debug.WriteLine("Empty WebResponse @'GetImage'" + Environment.NewLine + uri);

            return null;
        }

        /// <summary>
        /// Upload the specified file to the specified path.
        /// </summary>
        /// <param name="path">remote Path.</param>
        /// <param name="stream"></param>
        /// <param name="contentType">File content type.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns><c>true</c>, if upload successful, <c>false</c> otherwise.</returns>
        public Task<bool> Upload(string path, Stream stream, string contentType, IProgress<WebDavProgress> progress, CancellationToken cancellationToken)
        {
            return _dav.UploadFileWithProgressAsync(GetDavUri(path), stream, contentType, progress, cancellationToken);
        }

        /// <summary>
        /// Checks if the specified remote path exists.
        /// </summary>
        /// <param name="path">remote Path.</param>
        /// <returns><c>true</c>, if remote path exists, <c>false</c> otherwise.</returns>
        public Task<bool> Exists(string path)
        {
            return _dav.ExistsAsync(GetDavUri(path));
        }

        /// <summary>
        /// Creates a new directory at remote path.
        /// </summary>
        /// <returns><c>true</c>, if directory was created, <c>false</c> otherwise.</returns>
        /// <param name="path">remote Path.</param>
        public Task<bool> CreateDirectory(string path)
        {
            return _dav.CreateDirectoryAsync(GetDavUri(path));
        }

        /// <summary>
        ///     Delete resource at the specified remote path.
        /// </summary>
        /// <param name="path">remote Path.</param>
        /// <returns><c>true</c>, if resource was deleted, <c>false</c> otherwise.</returns>
        public Task<bool> Delete(string path)
        {
            return _dav.DeleteAsync(GetDavUri(path));
        }

        /// <summary>
        /// Copy the specified source to destination.
        /// </summary>
        /// <param name="source">Source resoure path.</param>
        /// <param name="destination">Destination resource path.</param>
        /// <returns><c>true</c>, if resource was copied, <c>false</c> otherwise.</returns>
        public Task<bool> Copy(string source, string destination)
        {
            return _dav.CopyAsync(GetDavUri(source), GetDavUri(destination));
        }

        /// <summary>
        /// Move the specified source and destination.
        /// </summary>
        /// <param name="source">Source resource path.</param>
        /// <param name="destination">Destination resource path.</param>
        /// <returns><c>true</c>, if resource was moved, <c>false</c> otherwise.</returns>
        public Task<bool> Move(string source, string destination)
        {
            return _dav.MoveAsync(GetDavUri(source), GetDavUri(destination));
        }

        /// <summary>
        /// Downloads a remote directory as zip.
        /// </summary>
        /// <param name="path">File remote Path.</param>
        /// <param name="localStream"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns>File contents.</returns>
        public Task<bool> DownloadDirectoryAsZip(string path, Stream localStream, IProgress<WebDavProgress> progress, CancellationToken cancellationToken)
        {
            return _dav.DownloadFileWithProgressAsync(GetDavUriZip(path), localStream, progress, cancellationToken);
        }

        public async Task<bool> ToggleFavorite(ResourceInfoModel res)
        {
            string path = GetParentPath(res);

            IList<WebDavSessionItem> items = await _dav.ListAsync(GetDavUri(path), _webDAVPropFind);
            WebDavSessionItem item = items.FirstOrDefault(x => x.Name == res.Name);

            if (item == null)
            {
                return false;
            }

            string favString = item.AdditionalProperties[WebDAVPropNameConstants.Favorite];

            if (string.IsNullOrEmpty(favString) || string.CompareOrdinal(favString, "0") == 0)
            {
                item.AdditionalProperties[WebDAVPropNameConstants.Favorite] = "1";
            }
            else
            {
                item.AdditionalProperties[WebDAVPropNameConstants.Favorite] = "0";
            }

            return await _dav.UpdateItemAsync(item);
        }

        private static string GetParentPath(ResourceInfoModel resourceInfo)
        {
            string path = resourceInfo.Path.TrimEnd('/');
            string[] split = path.Split('/');

            if (resourceInfo.IsDirectory)
            {
                path = string.Empty;
                for (int i = 1; i < split.Length - 1; i++)
                {
                    path += "/" + split[i];
                }
                //path = "/" + split[split.Length - 2];
            }

            //else
            //    path = "/" + split[split.Length - 1];

            return path;
        }

        #endregion

        #region WebDAV

        #region Remote Shares

        /// <summary>
        /// Gets the server status.
        /// </summary>
        /// <param name="serverUrl">The server URL.</param>
        /// <param name="ignoreServerCertificateErrors">if set to <c>true</c> [ignore server certificate errors].</param>
        /// <returns></returns>
        /// <exception cref="ResponseError">The certificate authority is invalid or incorrect
        /// or
        /// The remote server returned an error: (401) Unauthorized. - 401
        /// or</exception>
        public static async Task<Status> GetServerStatus(string serverUrl, bool ignoreServerCertificateErrors = false)
        {
            serverUrl = serverUrl.TrimEnd('/');
            if (serverUrl.EndsWith("owncloud") || serverUrl.EndsWith("nextcloud") || serverUrl.EndsWith("ownCloud"))
            {
                serverUrl += "/status.php";
            }
            else if (serverUrl.EndsWith("remote.php/webdav"))
            {
                serverUrl = serverUrl.Replace("remote.php/webdav", "status.php");
            }
            else
            {
                serverUrl += "/status.php";
            }
            string url;
            if (Uri.IsWellFormedUriString(serverUrl, UriKind.Absolute))
            {
                url = serverUrl;
            }
            else
            {
                return null;
            }

            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (ignoreServerCertificateErrors)
                {
                    // Specify which certificate errors should be ignored.
                    if (errors == System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    switch (errors)
                    {
                        case System.Net.Security.SslPolicyErrors.None:
                            return true;
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors:
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch:
                        case System.Net.Security.SslPolicyErrors.RemoteCertificateNotAvailable:
                        default:
                            return false;
                    }
                }
            };

            HttpClient client = new HttpClient(httpClientHandler);

            client.DefaultRequestHeaders.Add("Pragma", "no-cache");

            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("The certificate authority is invalid or incorrect"))
                {
                    throw new ResponseError("The certificate authority is invalid or incorrect");
                }
            }

            if (response == null)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ResponseError("The remote server returned an error: (401) Unauthorized.", "401");
            }

            string content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                throw new ResponseError(response.ReasonPhrase);
            }

            try
            {
                return JsonSerializer.Deserialize<Status>(content, _jsonSettings);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Checks the user login.
        /// </summary>
        /// <param name="serverUrl">The server URL.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static Task<bool> CheckUserLogin(string serverUrl, string userId, string password)
        {
            return CheckUserLogin(serverUrl, userId, password, false);
        }

        /// <summary>
        /// Checks the user login.
        /// </summary>
        /// <param name="serverUrl">The server URL.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="ignoreServerCertificateErrors">if set to <c>true</c> [ignore server certificate errors].</param>
        /// <returns></returns>
        public static async Task<bool> CheckUserLogin(string serverUrl, string userId, string password, bool ignoreServerCertificateErrors)
        {
            if (string.IsNullOrEmpty(serverUrl) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            // This method is also called on app reset.
            // Only using a HEAD request doesn't seem to work, because in subsequent calls (with wrong user/password), the server always returns HTTP 200 (OK).
            // So we're using an API call here.
            if (!serverUrl.EndsWith("/"))
            {
                serverUrl += "/";
            }

            Client client = new Client(serverUrl, userId, password, ignoreServerCertificateErrors);

            User user = null;

            try
            {
                user = await client.GetUserAttributes(userId);
            }
            catch
            {
                // ignored
            }

            return user != null;
        }

        /// <summary>
        /// List all remote shares.
        /// </summary>
        /// <returns>List of remote shares.</returns>
        public async Task<object> ListOpenRemoteShare()
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "remote_shares")
                );

            //Debug.Assert(response.StatusCode == HttpStatusCode.OK);

            // TODO: Parse response
            return response;
        }

        /// <summary>
        /// List all remote shares.
        /// </summary>
        /// <returns>List of remote shares.</returns>
        public async Task<object> ListShare()
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "shares")
                );

            //Debug.Assert(response.StatusCode == HttpStatusCode.OK);

            // TODO: Parse response
            return response;
        }

        /// <summary>
        /// Accepts a remote share
        /// </summary>
        /// <returns><c>true</c>, if remote share was accepted, <c>false</c> otherwise.</returns>
        /// <param name="shareId">Share identifier.</param>
        public async Task<bool> AcceptRemoteShare(int shareId)
        {
            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "remote_shares") + "/" + shareId
                );

            OCS responseObj = JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Declines a remote share.
        /// </summary>
        /// <returns><c>true</c>, if remote share was declined, <c>false</c> otherwise.</returns>
        /// <param name="shareId">Share identifier.</param>
        public async Task<bool> DeclineRemoteShare(int shareId)
        {
            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "remote_shares") + "/" + shareId
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        #endregion

        #region Shares

        /// <summary>
        /// Unshares a file or directory.
        /// </summary>
        /// <returns><c>true</c>, if share was deleted, <c>false</c> otherwise.</returns>
        /// <param name="shareId">Share identifier.</param>
        public async Task<bool> DeleteShare(int shareId)
        {
            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "remote_shares") + "/" + shareId
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Updates a given share. NOTE: Only one of the update parameters can be specified at once.
        /// </summary>
        /// <returns><c>true</c>, if share was updated, <c>false</c> otherwise.</returns>
        /// <param name="shareId">Share identifier.</param>
        /// <param name="perms">(optional) update permissions.</param>
        /// <param name="password">(optional) updated password for public link Share.</param>
        /// <param name="publicUpload">(optional) If set to <c>true</c> enables public upload for public shares.</param>
        public async Task<bool> UpdateShare(int shareId, int perms = -1, string password = null,
            OcsBoolParam publicUpload = OcsBoolParam.None)
        {
            if ((perms == Convert.ToInt32(OcsPermission.None)) && (password == null) &&
                (publicUpload == OcsBoolParam.None))
            {
                return false;
            }

            //var parameters = new List<KeyValuePair<string, string>>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (perms != Convert.ToInt32(OcsPermission.None))
            {
                parameters.Add("permissions", Convert.ToInt32(perms).ToString());
            }
            if (password != null)
            {
                parameters.Add("password", password);
            }
            switch (publicUpload)
            {
                case OcsBoolParam.True:
                    parameters.Add("publicUpload", "true");
                    break;
                case OcsBoolParam.False:
                    parameters.Add("publicUpload", "false");
                    break;
                case OcsBoolParam.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(publicUpload), publicUpload, null);
            }

            string response = await DoApiRequest(
                "PUT",
                "/" + GetOcsPath(OcsServiceShare, "shares") + "/" + shareId,
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Shares a remote file with link.
        /// </summary>
        /// <returns>instance of PublicShare with the share info.</returns>
        /// <param name="path">path to the remote file to share.</param>
        /// <param name="perms">(optional) permission of the shared object.</param>
        /// <param name="password">(optional) sets a password.</param>
        /// <param name="publicUpload">(optional) allows users to upload files or folders.</param>
        public async Task<PublicShare> ShareWithLink(string path, int perms = -1, string password = null,
            OcsBoolParam publicUpload = OcsBoolParam.None)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"shareType", Convert.ToInt32(OcsShareType.Link).ToString()},
                {"path", path}
            };

            if (perms != Convert.ToInt32(OcsPermission.None))
            {
                parameters.Add("permissions", Convert.ToInt32(perms).ToString());
            }
            if (password != null)
            {
                parameters.Add("password", password);
            }
            switch (publicUpload)
            {
                case OcsBoolParam.True:
                    parameters.Add("publicUpload", "true");
                    break;
                case OcsBoolParam.False:
                    parameters.Add("publicUpload", "false");
                    break;
                case OcsBoolParam.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(publicUpload), publicUpload, null);
            }

            string response = await DoApiRequest("POST", "/" + GetOcsPath(OcsServiceShare, "shares"));

            string responseStr = response;

            PublicShare share = new PublicShare
            {
                ShareId = Convert.ToInt32(GetFromData(responseStr, "id")),
                Url = GetFromData(responseStr, "url"),
                Token = GetFromData(responseStr, "token"),
                TargetPath = path,
                Perms = perms > -1 ? perms : Convert.ToInt32(OcsPermission.Read)
            };

            return share;
        }

        /// <summary>
        /// Shares a remote file with specified user.
        /// </summary>
        /// <returns>instance of UserShare with the share info.</returns>
        /// <param name="path">path to the remote file to share.</param>
        /// <param name="username">name of the user whom we want to share a file/folder.</param>
        /// <param name="perms">permissions of the shared object.</param>
        /// <param name="remoteUser">Remote user.</param>
        public async Task<object> ShareWithUser(string path, string username, int perms = -1,
            OcsBoolParam remoteUser = OcsBoolParam.None)
        {
            if ((perms == -1) || (perms > Convert.ToInt32(OcsPermission.All)) || string.IsNullOrEmpty(username))
            {
                return null;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"path", path},
                {"shareWith", username},
                {
                    "shareType", remoteUser == OcsBoolParam.True
                        ? Convert.ToInt32(OcsShareType.Remote).ToString()
                        : Convert.ToInt32(OcsShareType.User).ToString()
                },
                {
                    "permissions", perms != Convert.ToInt32(OcsPermission.None)
                        ? perms.ToString()
                        : Convert.ToInt32(OcsPermission.Read).ToString()
                }
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "shares"),
                parameters
                );

            string responseStr = response;

            UserShare share = new UserShare
            {
                ShareId = Convert.ToInt32(GetFromData(responseStr, "id")),
                TargetPath = path,
                Perms = perms,
                SharedWith = username
            };

            return share;
        }

        /// <summary>
        /// Shares a remote file with specified group.
        /// </summary>
        /// <returns>instance of GroupShare with the share info.</returns>
        /// <param name="path">path to the remote file to share.</param>
        /// <param name="groupName">name of the group whom we want to share a file/folder.</param>
        /// <param name="perms">permissions of the shared object.</param>
        public async Task<GroupShare> ShareWithGroup(string path, string groupName, int perms = -1)
        {
            if ((perms == -1) || (perms > Convert.ToInt32(OcsPermission.All)) || string.IsNullOrEmpty(groupName))
            {
                return null;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"shareType", Convert.ToInt32(OcsShareType.Group).ToString()},
                {"path", path},
                {
                    "permissions", perms != Convert.ToInt32(OcsPermission.None)
                        ? perms.ToString()
                        : Convert.ToInt32(OcsPermission.Read).ToString()
                },
                {"shareWith", groupName}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "shares"),
                parameters
                );

            string responseStr = response;

            GroupShare share = new GroupShare
            {
                ShareId = Convert.ToInt32(GetFromData(responseStr, "id")),
                TargetPath = path,
                Perms = perms,
                SharedWith = groupName
            };

            return share;
        }

        /// <summary>
        /// Checks whether a path is already shared.
        /// </summary>
        /// <returns><c>true</c> if this instance is shared the specified path; otherwise, <c>false</c>.</returns>
        /// <param name="path">path to the share to be checked.</param>
        public async Task<bool> IsShared(string path)
        {
            List<Share> result = await GetShares(new Tuple<string, string>("path", path));
            return result.Count > 0;
        }

        /// <summary>
        /// Gets all shares for the current user when <c>path</c> is not set, otherwise it gets shares for the specific file or
        /// folder
        /// </summary>
        /// <param name="tParam">The t parameter.</param>
        /// <param name="reshares">(optional) returns not only the shares from	the current user but all shares from the given file.</param>
        /// <param name="subfiles">(optional) returns all shares within	a folder, given that path defines a folder.</param>
        /// <returns>
        /// array of shares or empty array if the operation failed.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// reshares - null
        /// or
        /// subfiles - null
        /// </exception>
        public async Task<List<Share>> GetShares(Tuple<string, string> tParam, OcsBoolParam reshares = OcsBoolParam.None,
            OcsBoolParam subfiles = OcsBoolParam.None)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (tParam != null)
            {
                parameters.Add(tParam.Item1, tParam.Item2);
            }

            switch (reshares)
            {
                case OcsBoolParam.True:
                    parameters.Add("reshares", "true");
                    break;
                case OcsBoolParam.False:
                    parameters.Add("reshares", "false");
                    break;
                case OcsBoolParam.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reshares), reshares, null);
            }
            switch (subfiles)
            {
                case OcsBoolParam.True:
                    parameters.Add("subfiles", "true");
                    break;
                case OcsBoolParam.False:
                    parameters.Add("subfiles", "false");
                    break;
                case OcsBoolParam.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subfiles), subfiles, null);
            }

            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "shares"),
                parameters
                );

            string responseStr = response;

            return GetShareList(responseStr);
        }

        #endregion

        #region Users

        /// <summary>
        /// Create a new user with an initial password via provisioning API.
        /// </summary>
        /// <returns><c>true</c>, if user was created, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be created.</param>
        /// <param name="initialPassword">password for user being created.</param>
        public async Task<bool> CreateUser(string username, string initialPassword)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"userid", username},
                {"password", initialPassword}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "users"),
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Deletes a user via provisioning API.
        /// </summary>
        /// <returns><c>true</c>, if user was deleted, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be deleted.</param>
        public async Task<bool> DeleteUser(string username)
        {
            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }

            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Checks a user via provisioning API.
        /// </summary>
        /// <returns><c>true</c>, if exists was usered, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be checked.</param>
        public async Task<bool> UserExists(string username)
        {
            List<string> result = await SearchUsers(username);
            return result.Contains(username);
        }

        /// <summary>
        /// Searches for users via provisioning API.
        /// </summary>
        /// <returns>list of users.</returns>
        /// <param name="username">name of user to be searched for.</param>
        public async Task<List<string>> SearchUsers(string username)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"search", username}
            };

            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "users"),
                parameters
                );

            string responseStr = response;

            return GetDataElements(responseStr);
        }

        /// <summary>
        /// Gets the user's attributes.
        /// </summary>
        /// <returns>The user attributes.</returns>
        /// <param name="username">Username.</param>
        public async Task<User> GetUserAttributes(string username)
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceCloud, "users") + "/" + username
                );

            string responseStr = response;

            return GetUser(responseStr);
        }

        /// <summary>
        /// Gets the user's avatar.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<Uri> GetUserAvatarUrl(string username, int size)
        {
            Uri url = new Uri(_url + "/index.php/avatar/" + username + "/" + size);
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Pragma", "no-cache");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            if (!response.Headers.Contains("Content-Length"))
            {
                response = await client.GetAsync(url);
                if (response != null)
                {
                    Stream stream = (await response.Content.ReadAsStreamAsync());
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memStream);
                        return memStream.Length > 100 ? url : null;
                    }
                }
            }
            try
            {
                if (response != null)
                {
                    IEnumerable<string> strLength;
                    response.Headers.TryGetValues("Content-Length", out strLength);
                    long length;
                    long.TryParse(strLength.First(), out length);
                    if (length > 0)
                    {
                        return url;
                    }
                }
            }
            catch
            {
                return null;
            }
            return url;
        }

        /// <summary>
        /// Sets a user attribute. See
        /// https://doc.Nextcloud.com/server/7.0EE/admin_manual/configuration_auth_backends/user_provisioning_api.html#users-edituser
        /// for reference.
        /// </summary>
        /// <returns><c>true</c>, if user attribute was set, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to modify.</param>
        /// <param name="key">key of the attribute to set.</param>
        /// <param name="value">value to set.</param>
        public async Task<bool> SetUserAttribute(string username, string key, string value)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"key", key},
                {"value", value}
            };

            string response = await DoApiRequest(
                "PUT",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username,
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Adds a user to a group.
        /// </summary>
        /// <returns><c>true</c>, if user was added to group, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be added.</param>
        /// <param name="groupName">name of group user is to be added to.</param>
        public async Task<bool> AddUserToGroup(string username, string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"groupid", groupName}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/groups",
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Get a list of groups associated to a user.
        /// </summary>
        /// <returns>list of groups.</returns>
        /// <param name="username">name of user to list groups.</param>
        public async Task<List<string>> GetUserGroups(string username)
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/groups"
                );

            string responseStr = response;

            return GetDataElements(responseStr);
        }

        /// <summary>
        /// Check if a user is in a group.
        /// </summary>
        /// <returns><c>true</c>, if user is in group, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user.</param>
        /// <param name="groupName">name of group.</param>
        public async Task<bool> IsUserInGroup(string username, string groupName)
        {
            List<string> groups = await GetUserGroups(username);
            return groups.Contains(groupName);
        }

        /// <summary>
        /// Removes a user from a group.
        /// </summary>
        /// <returns><c>true</c>, if user was removed from group, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be removed.</param>
        /// <param name="groupName">name of group user is to be removed from.</param>
        public async Task<bool> RemoveUserFromGroup(string username, string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"groupid", groupName}
            };

            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/groups",
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Adds a user to a subadmin group.
        /// </summary>
        /// <returns><c>true</c>, if user was added to sub admin group, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user to be added to subadmin group.</param>
        /// <param name="groupName">name of subadmin group.</param>
        public async Task<bool> AddUserToSubAdminGroup(string username, string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"groupid", groupName}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/subadmins",
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Get a list of subadmin groups associated to a user.
        /// </summary>
        /// <returns>list of subadmin groups.</returns>
        /// <param name="username">name of user.</param>
        public async Task<List<string>> GetUserSubAdminGroups(string username)
        {
            string responseStr = null;

            try
            {
                string response = await DoApiRequest(
                    "GET",
                    "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/subadmins"
                    );

                responseStr = response;
            }
            catch (OcsResponseError ocserr)
            {
                if (ocserr.StatusCode.Equals("102")) // empty response results in a OCS 102 Error
                {
                    return new List<string>();
                }
            }

            return GetDataElements(responseStr);
        }

        /// <summary>
        /// Check if a user is in a subadmin group.
        /// </summary>
        /// <returns><c>true</c>, if user is in sub admin group, <c>false</c> otherwise.</returns>
        /// <param name="username">name of user.</param>
        /// <param name="groupNname">name of subadmin group.</param>
        public async Task<bool> IsUserInSubAdminGroup(string username, string groupNname)
        {
            List<string> groups = await GetUserSubAdminGroups(username);
            return groups.Contains(groupNname);
        }

        /// <summary>
        /// Removes the user from sub admin group.
        /// </summary>
        /// <returns><c>true</c>, if user from sub admin group was removed, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        /// <param name="groupName">Group name.</param>
        public async Task<bool> RemoveUserFromSubAdminGroup(string username, string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"groupid", groupName}
            };

            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "users") + "/" + username + "/subadmins",
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        #endregion

        #region Groups

        /// <summary>
        /// Create a new group via provisioning API.
        /// </summary>
        /// <returns><c>true</c>, if group was created, <c>false</c> otherwise.</returns>
        /// <param name="groupName">name of group to be created.</param>
        public async Task<bool> CreateGroup(string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"groupid", groupName}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "groups"),
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <returns><c>true</c>, if group was deleted, <c>false</c> otherwise.</returns>
        /// <param name="groupName">Group name.</param>
        public async Task<bool> DeleteGroup(string groupName)
        {
            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "groups") + "/" + groupName
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Checks a group via provisioning API.
        /// </summary>
        /// <returns><c>true</c>, if group exists, <c>false</c> otherwise.</returns>
        /// <param name="groupName">name of group to be checked.</param>
        public async Task<bool> GroupExists(string groupName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"search", groupName}
            };

            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "groups"),
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        #endregion

        #region Config

        /// <summary>
        /// Returns Nextcloud config information.
        /// </summary>
        /// <returns>The config.</returns>
        public async Task<Config> GetConfig()
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath("", "config")
                );

            string responseStr = response;

            Config cfg = new Config
            {
                Contact = GetFromData(responseStr, "contact"),
                Host = GetFromData(responseStr, "host"),
                Ssl = GetFromData(responseStr, "ssl"),
                Version = GetFromData(responseStr, "version"),
                website = GetFromData(responseStr, "website")
            };

            return cfg;
        }

        #endregion

        #region Application attributes

        /// <summary>
        /// Returns an application attribute
        /// </summary>
        /// <returns>App Attribute List.</returns>
        /// <param name="app">application id.</param>
        /// <param name="key">attribute key or None to retrieve all values for the given application.</param>
        public async Task<List<AppAttribute>> GetAttribute(string app = "", string key = "")
        {
            string path = "getattribute";
            if (!app.Equals(""))
            {
                path += "/" + app;
                if (!key.Equals(""))
                {
                    path += "/" + System.Net.WebUtility.UrlEncode(key);
                }
            }

            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceData, path)
                );

            string responseStr = response;

            return GetAttributeList(responseStr);
        }

        /// <summary>
        /// Sets an application attribute.
        /// </summary>
        /// <returns><c>true</c>, if attribute was set, <c>false</c> otherwise.</returns>
        /// <param name="app">application id.</param>
        /// <param name="key">key of the attribute to set.</param>
        /// <param name="value">value to set.</param>
        public async Task<bool> SetAttribute(string app, string key, string value)
        {
            string path = "setattribute" + "/" + app + "/" + System.Net.WebUtility.UrlEncode(key);

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"value", value}
            };

            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceData, path),
                parameters
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Deletes an application attribute.
        /// </summary>
        /// <returns><c>true</c>, if attribute was deleted, <c>false</c> otherwise.</returns>
        /// <param name="app">application id.</param>
        /// <param name="key">key of the attribute to delete.</param>
        public async Task<bool> DeleteAttribute(string app, string key)
        {
            string path = "deleteattribute" + "/" + app + "/" + System.Net.WebUtility.UrlEncode(key);

            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceData, path)
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        #endregion

        #region Apps

        /// <summary>
        /// List all enabled apps through the provisioning api.
        /// </summary>
        /// <returns>a list of apps and their enabled state.</returns>
        public async Task<List<string>> GetApps()
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "apps")
                );

            string responseStr = response;

            return GetDataElements(responseStr);
        }

        /// <summary>
        /// Gets information about the specified app.
        /// </summary>
        /// <returns>App information.</returns>
        /// <param name="appName">App name.</param>
        public async Task<AppInfo> GetApp(string appName)
        {
            string response = await DoApiRequest(
                "GET",
                "/" + GetOcsPath(OcsServiceShare, "apps") + "/" + appName
                );

            string responseStr = response;

            return GetAppInfo(responseStr);
        }

        /// <summary>
        /// Enable an app through provisioning_api.
        /// </summary>
        /// <returns><c>true</c>, if app was enabled, <c>false</c> otherwise.</returns>
        /// <param name="appName">Name of app to be enabled.</param>
        public async Task<bool> EnableApp(string appName)
        {
            string response = await DoApiRequest(
                "POST",
                "/" + GetOcsPath(OcsServiceShare, "apps") + "/" + appName
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        /// <summary>
        /// Disable an app through provisioning_api
        /// </summary>
        /// <returns><c>true</c>, if app was disabled, <c>false</c> otherwise.</returns>
        /// <param name="appName">Name of app to be disabled.</param>
        public async Task<bool> DisableApp(string appName)
        {
            string response = await DoApiRequest(
                "DELETE",
                "/" + GetOcsPath(OcsServiceShare, "apps") + "/" + appName
                );

            OCS responseObj =
                JsonSerializer.Deserialize<OCS>(response, _jsonSettings);

            if (responseObj == null)
            {
                return false;
            }
            if (responseObj.Meta.StatusCode == 100)
            {
                return true;
            }
            throw new OcsResponseError(responseObj.Meta.Message, responseObj.Meta.StatusCode.ToString());
        }

        #endregion

        #endregion

        #region Url Handling

        private async Task<string> DoApiRequest(string method, string path, Dictionary<string, string> parameters = null)
        {
            UrlBuilder url = new UrlBuilder(_url + "/" + Ocspath + path);
            HttpResponseMessage response;
            switch (method)
            {
                case "GET":
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, string> parameter in parameters)
                        {
                            url.AddQueryParameter(parameter.Key, parameter.Value);
                        }
                    }
                    _client.DefaultRequestHeaders.Add("OCS-APIREQUEST", "true");
                    response = await _client.GetAsync(url.ToUri());
                    _client.DefaultRequestHeaders.Remove("OCS-APIREQUEST");
                    break;

                default:
                    FormUrlEncodedContent content2 = new FormUrlEncodedContent(parameters);
                    content2.Headers.Add("OCS-APIREQUEST", "true");
                    response = await _client.PostAsync(url.ToUri(), content2);
                    break;
            }
            string message = await response.Content.ReadAsStringAsync();
            CheckOcsStatus(response, message);

            return message;
        }

        /// <summary>
        /// Gets the DAV request URI.
        /// </summary>
        /// <returns>The DAV URI.</returns>
        /// <param name="path">remote Path.</param>
        /// <param name="fullPath">Determines if the path is complete.</param>
        private Uri GetDavUri(string path, bool fullPath)
        {
            if (fullPath)
            {
                return new Uri(UriHelper.CombineUrl(UriHelper.CombineUrl(_url, Davpath, true), path, true));
            }
            else
            {
                return new Uri(UriHelper.CombineUrl(Davpath, path, true), UriKind.Relative);
            }
        }
        /// <summary>
        /// Gets the DAV request URI.
        /// </summary>
        /// <param name="path">remote Path.</param>
        /// <returns></returns>
        private Uri GetDavUri(string path)
        {
            return GetDavUri(path, false);
        }

        /// <summary>
        /// Gets the DAV request URI.
        /// </summary>
        /// <returns>The DAV URI.</returns>
        /// <param name="path">remote Path.</param>
        private Uri GetDavUriZip(string path)
        {
            string[] pathArry = path.Split('/');
            string files = pathArry[pathArry.Length - 2];
            path = path.Substring(0, path.Length - (files.Length + 1));

            UrlBuilder url = new UrlBuilder(_url + "/index.php/apps/files/ajax/download.php");
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"dir", path},
                {"files", files}
            };
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                url.AddQueryParameter(parameter.Key, parameter.Value);
            }
            return url.ToUri();
        }

        /// <summary>
        /// Gets the remote path for OCS API.
        /// </summary>
        /// <returns>The ocs path.</returns>
        /// <param name="service">Service.</param>
        /// <param name="action">Action.</param>
        private string GetOcsPath(string service, string action)
        {
            string slash = string.IsNullOrEmpty(service) ? "" : "/";
            return service + slash + action;
        }

        #endregion

        #region OCS Response parsing

        /// <summary>
        /// Get element value from OCS Meta.
        /// </summary>
        /// <returns>Element value.</returns>
        /// <param name="response">XML OCS response.</param>
        /// <param name="elementName">XML Element name.</param>
        private static string GetFromMeta(string response, string elementName)
        {
            XDocument xdoc = XDocument.Parse(response);

            return (from data in xdoc.Descendants(XName.Get("meta"))
                    select data.Element(XName.Get(elementName))
                into node
                    where node != null
                    select node.Value).FirstOrDefault();
        }

        /// <summary>
        /// Get element value from OCS Data.
        /// </summary>
        /// <returns>Element value.</returns>
        /// <param name="response">XML OCS response.</param>
        /// <param name="elementName">XML Element name.</param>
        private string GetFromData(string response, string elementName)
        {
            XDocument xdoc = XDocument.Parse(response);

            return (from data in xdoc.Descendants(XName.Get("data"))
                    select data.Element(XName.Get(elementName))
                into node
                    where node != null
                    select node.Value).FirstOrDefault();
        }

        /// <summary>
        /// Gets the data element values.
        /// </summary>
        /// <returns>The data elements.</returns>
        /// <param name="response">XML OCS Response.</param>
        private List<string> GetDataElements(string response)
        {
            XDocument xdoc = XDocument.Parse(response);

            return
                (from data in xdoc.Descendants(XName.Get("data"))
                 from node in data.Descendants(XName.Get("element"))
                 select node.Value).ToList();
        }

        /// <summary>
        /// Gets the share list from a OCS Data response.
        /// </summary>
        /// <returns>The share list.</returns>
        /// <param name="response">XML OCS Response.</param>
        private List<Share> GetShareList(string response)
        {
            List<Share> shares = new List<Share>();
            XDocument xdoc = XDocument.Parse(response);

            foreach (XElement data in xdoc.Descendants(XName.Get("element")))
            {
                XElement node = data.Element(XName.Get("share_type"));
                if (node == null)
                {
                    continue;
                }

                #region Share Type

                int shareType = Convert.ToInt32(node.Value);
                Share share;
                if (shareType == Convert.ToInt32(OcsShareType.Link))
                {
                    share = new PublicShare();
                }
                else if (shareType == Convert.ToInt32(OcsShareType.User))
                {
                    share = new UserShare();
                }
                else if (shareType == Convert.ToInt32(OcsShareType.Group))
                {
                    share = new GroupShare();
                }
                else
                {
                    share = new Share();
                }
                share.AdvancedProperties = new AdvancedShareProperties();

                #endregion

                #region General Properties

                node = data.Element(XName.Get("id"));
                if (node != null)
                {
                    share.ShareId = Convert.ToInt32(node.Value);
                }

                node = data.Element(XName.Get("file_target"));
                if (node != null)
                {
                    share.TargetPath = node.Value;
                }

                node = data.Element(XName.Get("path"));
                if (node != null)
                {
                    share.Path = node.Value;
                }

                node = data.Element(XName.Get("permissions"));
                if (node != null)
                {
                    share.Perms = Convert.ToInt32(node.Value);
                }

                #endregion

                #region Advanced Properties

                node = data.Element(XName.Get("item_type"));
                if (node != null)
                {
                    share.AdvancedProperties.ItemType = node.Value;
                }

                node = data.Element(XName.Get("item_source"));
                if (node != null)
                {
                    share.AdvancedProperties.ItemSource = node.Value;
                }

                node = data.Element(XName.Get("parent"));
                if (node != null)
                {
                    share.AdvancedProperties.Parent = node.Value;
                }

                node = data.Element(XName.Get("file_source"));
                if (node != null)
                {
                    share.AdvancedProperties.FileSource = node.Value;
                }

                node = data.Element(XName.Get("stime"));
                if (node != null)
                {
                    share.AdvancedProperties.STime = node.Value;
                }

                node = data.Element(XName.Get("expiration"));
                if (node != null)
                {
                    share.AdvancedProperties.Expiration = node.Value;
                }

                node = data.Element(XName.Get("mail_send"));
                if (node != null)
                {
                    share.AdvancedProperties.MailSend = node.Value;
                }

                node = data.Element(XName.Get("uid_owner"));
                if (node != null)
                {
                    share.AdvancedProperties.Owner = node.Value;
                }

                node = data.Element(XName.Get("storage_id"));
                if (node != null)
                {
                    share.AdvancedProperties.StorageId = node.Value;
                }

                node = data.Element(XName.Get("storage"));
                if (node != null)
                {
                    share.AdvancedProperties.Storage = node.Value;
                }

                node = data.Element(XName.Get("file_parent"));
                if (node != null)
                {
                    share.AdvancedProperties.FileParent = node.Value;
                }

                node = data.Element(XName.Get("share_with_displayname"));
                if (node != null)
                {
                    share.AdvancedProperties.ShareWithDisplayname = node.Value;
                }

                node = data.Element(XName.Get("displayname_owner"));
                if (node != null)
                {
                    share.AdvancedProperties.DisplaynameOwner = node.Value;
                }

                #endregion

                #region ShareType specific

                if (shareType == Convert.ToInt32(OcsShareType.Link))
                {
                    node = data.Element(XName.Get("url"));
                    if (node != null)
                    {
                        ((PublicShare)share).Url = node.Value;
                    }

                    node = data.Element(XName.Get("token"));
                    if (node != null)
                    {
                        ((PublicShare)share).Token = node.Value;
                    }
                }
                else if (shareType == Convert.ToInt32(OcsShareType.User))
                {
                    node = data.Element(XName.Get("share_with"));
                    if (node != null)
                    {
                        ((UserShare)share).SharedWith = node.Value;
                    }
                }
                else if (shareType == Convert.ToInt32(OcsShareType.Group))
                {
                    node = data.Element(XName.Get("share_with"));
                    if (node != null)
                    {
                        ((GroupShare)share).SharedWith = node.Value;
                    }
                }

                #endregion

                shares.Add(share);
            }

            return shares;
        }

        /// <summary>
        /// Checks the validity of the OCS Request. If invalid a exception is thrown.
        /// </summary>
        /// <param name="response">OCS Response.</param>
        /// <exception cref="ResponseError">Empty response</exception>
        /// <exception cref="OcsResponseError"></exception>
        private static void CheckOcsStatus(HttpResponseMessage response, string xml)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ResponseError("The remote server returned an error: (401) Unauthorized.", "401");
            }
            if (response.Content == null)
            {
                throw new ResponseError(response.ReasonPhrase);
            }
            string ocsStatus = GetFromMeta(xml, "statuscode");
            if (ocsStatus == null)
            {
                throw new ResponseError("Empty response");
            }
            if (!ocsStatus.Equals("100"))
            {
                throw new OcsResponseError(GetFromMeta(xml, "message"), ocsStatus);
            }
        }

        /// <summary>
        /// Returns a list of application attributes.
        /// </summary>
        /// <param name="response">XML OCS Response.</param>
        /// <returns>
        /// List of application attributes.
        /// </returns>
        private List<AppAttribute> GetAttributeList(string response)
        {
            List<AppAttribute> result = new List<AppAttribute>();
            XDocument xdoc = XDocument.Parse(response);

            foreach (XElement data in xdoc.Descendants(XName.Get("data")))
            {
                foreach (XElement element in data.Descendants(XName.Get("element")))
                {
                    AppAttribute attr = new AppAttribute();

                    XElement node = element.Element(XName.Get("app"));
                    if (node != null)
                    {
                        attr.App = node.Value;
                    }

                    node = element.Element(XName.Get("key"));
                    if (node != null)
                    {
                        attr.Key = node.Value;
                    }

                    node = element.Element(XName.Get("value"));
                    if (node != null)
                    {
                        attr.value = node.Value;
                    }

                    result.Add(attr);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the user attributes from a OCS XML Response.
        /// </summary>
        /// <returns>The user attributes.</returns>
        /// <param name="response">OCS XML Response.</param>
        private User GetUser(string response)
        {
            User user = new User();
            XDocument xdoc = XDocument.Parse(response);

            XElement data = xdoc.Descendants(XName.Get("data")).FirstOrDefault();
            if (data == null)
            {
                return user;
            }
            XElement node = data.Element(XName.Get("displayname"));
            if (node != null)
            {
                user.DisplayName = node.Value;
            }

            node = data.Element(XName.Get("email"));
            if (node != null)
            {
                user.EMail = node.Value;
            }

            node = data.Element(XName.Get("enabled"));
            if (node != null)
            {
                user.Enabled = node.Value.Equals("true");
            }

            Quota quota = new Quota();
            user.Quota = quota;

            XElement element = data.Descendants(XName.Get("quota")).FirstOrDefault();
            if (element == null)
            {
                return user;
            }

            node = element.Element(XName.Get("free"));
            if (node != null)
            {
                quota.Free = Convert.ToInt64(node.Value);
            }

            node = element.Element(XName.Get("used"));
            if (node != null)
            {
                quota.Used = Convert.ToInt64(node.Value);
            }

            node = element.Element(XName.Get("total"));
            if (node != null)
            {
                quota.Total = Convert.ToInt64(node.Value);
            }

            node = element.Element(XName.Get("relative"));
            if (node != null)
            {
                quota.Relative = Convert.ToDouble(node.Value);
            }

            node = element.Element(XName.Get("quota"));
            if (node != null)
            {
                quota.QuotaValue = Convert.ToInt64(node.Value);
            }

            return user;
        }

        /// <summary>
        /// Gets the application information.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private AppInfo GetAppInfo(string response)
        {
            AppInfo app = new AppInfo();
            XDocument xdoc = XDocument.Parse(response);

            foreach (XElement data in xdoc.Descendants(XName.Get("data")))
            {
                XElement node = data.Element(XName.Get("id"));
                if (node != null)
                {
                    app.Id = node.Value;
                }

                node = data.Element(XName.Get("name"));
                if (node != null)
                {
                    app.Name = node.Value;
                }

                node = data.Element(XName.Get("description"));
                if (node != null)
                {
                    app.Description = node.Value;
                }

                node = data.Element(XName.Get("licence"));
                if (node != null)
                {
                    app.Licence = node.Value;
                }

                node = data.Element(XName.Get("author"));
                if (node != null)
                {
                    app.Author = node.Value;
                }

                node = data.Element(XName.Get("requiremin"));
                if (node != null)
                {
                    app.RequireMin = node.Value;
                }

                node = data.Element(XName.Get("shipped"));
                if (node != null)
                {
                    app.Shipped = node.Value.Equals("true");
                }

                node = data.Element(XName.Get("standalone"));
                app.Standalone = node != null;

                node = data.Element(XName.Get("default_enable"));
                app.DefaultEnable = node != null;

                node = data.Element(XName.Get("types"));
                if (node != null)
                {
                    app.Types = XmlElementsToList(node);
                }

                node = data.Element(XName.Get("remote"));
                if (node != null)
                {
                    app.Remote = XmlElementsToDict(node);
                }

                node = data.Element(XName.Get("documentation"));
                if (node != null)
                {
                    app.Documentation = XmlElementsToDict(node);
                }

                node = data.Element(XName.Get("info"));
                if (node != null)
                {
                    app.Info = XmlElementsToDict(node);
                }

                node = data.Element(XName.Get("public"));
                if (node != null)
                {
                    app.Public = XmlElementsToDict(node);
                }
            }

            return app;
        }

        /// <summary>
        /// Returns the elements of a XML Element as a List.
        /// </summary>
        /// <returns>The elements as list.</returns>
        /// <param name="element">XML Element.</param>
        private List<string> XmlElementsToList(XContainer element)
        {
            return element.Descendants(XName.Get("element")).Select(node => node.Value).ToList();
        }

        /// <summary>
        /// Returns the elements of a XML Element as a Dictionary.
        /// </summary>
        /// <returns>The elements as dictionary.</returns>
        /// <param name="element">XML Element.</param>
        private Dictionary<string, string> XmlElementsToDict(XContainer element)
        {
            return element.Descendants().ToDictionary(node => node.Name.ToString(), node => node.Value);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dav?.Dispose();
            }
        }

        #endregion IDisposable
    }
}