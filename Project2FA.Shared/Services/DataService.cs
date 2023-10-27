using Project2FA.Repository.Models;
using System;
using System.Collections.ObjectModel;
using Prism.Ioc;
using Project2FA.Core.Services.JSON;
using Windows.Storage;
using System.Collections.Specialized;
using OtpNet;
using Project2FA.Core.Utils;
using System.Threading;
using System.Security.Cryptography;
using Project2FA.Strings;
using Windows.ApplicationModel.Core;
using System.IO;
using Project2FA.Core;
using Project2FA.Core.Services.NTP;
using System.Threading.Tasks;
using DecaTec.WebDav;
using Project2FA.Services.WebDAV;
using Windows.Storage.Streams;
using WebDAVClient.Types;
using System.Collections.Generic;
using Project2FA.Helpers;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Core.Messenger;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using Project2FA.Repository.Models.Enums;
using UNOversal.Services.Secrets;
using UNOversal.Services.Dialogs;
using UNOversal.Logging;
using UNOversal.Services.File;
using UNOversal.Services.Network;
using Project2FA.Utils;
using Project2FA.Core.Services.Crypto;
using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Helpers;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Collections;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Project2FA.UWP.Utils;
using Windows.Security.Authorization.AppCapabilityAccess;
#else


using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Services
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class DataService : ObservableRecipient, IDisposable
    {
        private SemaphoreSlim _collectionAccessSemaphore;
        public SemaphoreSlim CollectionAccessSemaphore 
        { 
            get
            {
                if (_collectionAccessSemaphore == null)
                {
                    _collectionAccessSemaphore = new SemaphoreSlim(1);
                }
                return _collectionAccessSemaphore;
            }
        }
        private bool _checkedTimeSynchronisation;
        private bool _isLoading;
        private TimeSpan _ntpServerTimeDifference;
        private ISecretService SecretService { get; }
        private IDialogService DialogService { get; }
        private ILoggerFacade Logger { get; }
        private IFileService FileService { get; }
        private INetworkTimeService NetworkTimeService { get; }
        private INetworkService NetworkService { get; }
        private bool _isFirstActivatedDatafileStart = true;
        private bool _initialization, _errorOccurred;
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        public Stopwatch TOTPEventStopwatch { get; }
        public AdvancedCollectionView ACVCollection { get; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();

        public ObservableCollection<CategoryModel> GlobalCategories { get; set; } = new ObservableCollection<CategoryModel>();
        private StorageFile _openDatefile;
        private bool _emptyAccountCollectionTipIsOpen;
        private TwoFACodeModel _tempDeletedTFAModel;
        private const long unixEpochTicks = 621355968000000000L;
        private const long ticksToSeconds = 10000000L;
        int _reloadCollectionCounter = 0;
#if __IOS__
        private Foundation.NSUrl _openDatefileUrl = null;
#endif

        //private StorageFileQueryResult _queryResult; // to reload the datafile if the file is modified
        //private bool _datafileWritten;
        //private int _queryChangedCounter;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static DataService Instance { get; } = new DataService();

        /// <summary>
        /// Private Constructor
        /// </summary>
        private DataService()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            Logger = App.Current.Container.Resolve<ILoggerFacade>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            NewtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            NetworkTimeService = App.Current.Container.Resolve<INetworkTimeService>();
            NetworkService = App.Current.Container.Resolve<INetworkService>();
            ACVCollection = new AdvancedCollectionView(Collection, true);
            TOTPEventStopwatch = new Stopwatch();
            //ACVCollection.SortDescriptions.Add(new SortDescription("Label", SortDirection.Ascending));
            ACVCollection.SortDescriptions.Add(new SortDescription("IsFavouriteText", SortDirection.Ascending));
            Collection.CollectionChanged += Accounts_CollectionChanged;
            CheckTime().ConfigureAwait(false);
        }

        public async Task StartService()
        {
            await CheckLocalDatafile();
            TOTPEventStopwatch.Start();
        }

        /// <summary>
        /// Check if the current time of the system is correct
        /// </summary>
        private async Task CheckTime()
        {
            TimeSpan difference = DateTime.UtcNow - SettingsService.Instance.LastCheckedSystemTime;
            // check the time again after 8 hours
            if (difference.TotalHours > 8)
            {
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable && SettingsService.Instance.UseNTPServerCorrection)
                {
                    try
                    {
                        DateTime time = await NetworkTimeService.GetNetworkTimeAsync(SettingsService.Instance.NTPServerString);
                        TimeSpan timespan = time.Subtract(DateTime.UtcNow);
                        _checkedTimeSynchronisation = true;
                        if (Math.Abs(timespan.TotalSeconds) >= 15) // difference of 15 seconds or more
                        {
                            _ntpServerTimeDifference = timespan;
                            await ErrorDialogs.SystemTimeNotCorrectError();
                        }
                        SettingsService.Instance.LastCheckedSystemTime = DateTime.UtcNow;
                    }
                    catch (Exception exc)
                    {
                        Logger.Log("NTP exception: " + exc.Message, Category.Exception, Priority.Low);
                        //TrackingManager.TrackException(exc);
                    }
                }
            }
        }

        /// <summary>
        /// Create TOTP code for collection initialization and write the datafile, if an item added or removed from the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Remove:
                    // normal mode
                    if (_initialization == false)
                    {
                        await WriteLocalDatafile();
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            await ResetCollection();
                        }
                    }
                    // if the initialization is active
                    else
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            var useHiddenTOTP = SettingsService.Instance.UseHiddenTOTP;
                            // initialize the newest (last) item
                            int i = (sender as ObservableCollection<TwoFACodeModel>).Count - 1;
                            Collection[i].HideTOTPCode = useHiddenTOTP;
                            // set the svg source
                            await SVGColorHelper.GetSVGIconWithThemeColor(Collection[i], Collection[i].AccountIconName);
                            await InitializeItem(i);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reset the TOTP code
        /// </summary>
        public async Task ResetCollection()
        {
            var useHiddenTOTP = SettingsService.Instance.UseHiddenTOTP;
            await CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < Collection.Count; i++)
            {
                Collection[i].HideTOTPCode = useHiddenTOTP;
                await InitializeItem(i);
            }
            TOTPEventStopwatch.Restart();
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Reset the period and regnerate the TOTP code
        /// </summary>
        /// <param name="i">The index of the item in the collection</param>
        private Task InitializeItem(int i)
        {
            return GenerateTOTP(i);
        }

        /// <summary>
        /// Reloads the datafile in the local database
        /// </summary>
        public async Task ReloadDatafile()
        {
            await CheckLocalDatafile();
        }

        /// <summary>
        /// Checks and reads the current local datafile
        /// </summary>
        private async Task CheckLocalDatafile()
        {
            if (Collection.Count == 0)
            {
                // loading animation only for empty collection
                IsLoading = true;
            }

            DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
            try
            {
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();

#if !WINDOWS_UWP
                // not for UWP
                StorageFile file = null;
#endif
#if __IOS__
                Foundation.NSUrl nsUrl = null;
#endif
                StorageFolder folder = null;
                string datafilename, passwordHashName, path;
                DBPasswordHashModel dbHash;

                if (ActivatedDatafile != null)
                {
#if WINDOWS_UWP
                    // check if the app has file system access to load the file
                    var checkFileSystemAccess = AppCapability.Create(Constants.BroadFileSystemAccessName).CheckAccess();
                    switch (checkFileSystemAccess)
                    {
                        case AppCapabilityAccessStatus.DeniedBySystem:
                        case AppCapabilityAccessStatus.NotDeclaredByApp:
                        case AppCapabilityAccessStatus.DeniedByUser:
                        case AppCapabilityAccessStatus.UserPromptRequired:
                            await ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                            return;
                        case AppCapabilityAccessStatus.Allowed:
                            break;
                        default:
                            await ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                            break;
                    }
#endif
                    path = ActivatedDatafile.Path;
                    dbHash = null;
                    passwordHashName = Constants.ActivatedDatafileHashName;
                    datafilename = ActivatedDatafile.Name;
#if WINDOWS_UWP
                    folder = await ActivatedDatafile.GetParentAsync();
#endif
#if __IOS__
                    nsUrl = OpenDatefileUrl;
#endif
#if __IOS__ || __ANDROID__
                    file = ActivatedDatafile;
#endif
                }
                else
                {
#if WINDOWS_UWP
                    folder = dbDatafile.IsWebDAV ?
                    ApplicationData.Current.LocalFolder :
                    await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
#else
                    path = dbDatafile.Path;
                    if (dbDatafile.IsWebDAV)
                    {
                        folder = ApplicationData.Current.LocalFolder;
                    }
#endif

                    dbHash = await App.Repository.Password.GetAsync();
                    passwordHashName = dbHash.Hash;
                    datafilename = dbDatafile.Name;

#if __ANDROID__
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
                    await Task.Run(async () => {
                        Android.Net.Uri androidUri = Android.Net.Uri.Parse(path);
                        file = StorageFile.GetFromSafUri(androidUri);
                    });
#endif
#if __IOS__
                    Foundation.NSData bookmark = Foundation.NSUserDefaults.StandardUserDefaults.DataForKey(Constants.ContainerName);
                    nsUrl = Foundation.NSUrl.FromBookmarkData(
                        bookmark,
                        Foundation.NSUrlBookmarkResolutionOptions.WithSecurityScope,
                        null,
                        out bool isStale,
                        out Foundation.NSError error);
                    file = await StorageFile.GetFileFromPathAsync(nsUrl.Path);
#endif
                }




#if __ANDROID__ || __IOS__
                if (file != null)
#endif
#if WINDOWS_UWP
                if (await FileService.FileExistsAsync(datafilename, folder))
#endif
                {
                    await CollectionAccessSemaphore.WaitAsync();
                    // prevent write of the datafile to folder
                    _initialization = true;
                    try
                    {
                        string datafileStr = string.Empty;

#if __ANDROID__
                        // create new thread for buggy Android, else NetworkOnMainThreadException 
                        await Task.Run(async () => {
                            datafileStr = await FileIO.ReadTextAsync(file);
                        });
                        
#endif
#if __IOS__
                        if (Foundation.NSFileManager.DefaultManager.IsReadableFile(nsUrl.Path))
                        {
                            datafileStr = await FileIO.ReadTextAsync(file);
                        }
                        else
                        {
                            if (nsUrl.StartAccessingSecurityScopedResource())
                            {
                                datafileStr = await FileIO.ReadTextAsync(file);
                            }
                        }
#endif
#if WINDOWS_UWP
                        datafileStr = await FileService.ReadStringAsync(datafilename, folder);
#endif


                        if (!string.IsNullOrWhiteSpace(datafileStr))
                        {
                            // read the iv for AES
                            DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            GlobalCategories.Clear();
                            if (datafile.GlobalCategories != null)
                            {
                                GlobalCategories.AddRange(datafile.GlobalCategories);
                            }
                            byte[] iv = datafile.IV;

                            if (ActivatedDatafile != null)
                            {
#if WINDOWS_UWP
                                datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                    ProtectData.Unprotect(NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName))),
                                    iv,
                                    datafileStr,
                                    datafile.Version);
#else
                                datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                    NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName)),
                                    iv,
                                    datafileStr,
                                    datafile.Version);
#endif
                            }
                            else
                            {
                                datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                    SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName),
                                    iv,
                                    datafileStr,
                                    datafile.Version);
                            }


                            deserializeCollection = datafile.Collection;
                        }
                        if (deserializeCollection != null)
                        {
                            if (Collection.AddRange(deserializeCollection, true) == 0) // clear the current items and add the new
                            {
                                // if no error has occured
                                if (!_errorOccurred)
                                {
                                    EmptyAccountCollectionTipIsOpen = true;
                                }
                            }
                            else
                            {
                                if (EmptyAccountCollectionTipIsOpen)
                                {
                                    EmptyAccountCollectionTipIsOpen = false;
                                }
                            }
                            

                            //if (_queryResult == null)
                            //{
                            //    try
                            //    {
                            //        // monitors the folder of the datafile for changes and triggers the reload. 
                            //        List<string> fileTypeFilter = new List<string>();
                            //        fileTypeFilter.Add(".2fa");
                            //        var options = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);
                            //        _queryResult = folder.CreateFileQueryWithOptions(options);
                            //        //subscribe on query's ContentsChanged event
                            //        _queryResult.ContentsChanged += Query_DatafileChanged;
                            //        // call the query to get later changed elements
                            //        //TODO add this feature
                            //        //var files = await _queryResult.GetFilesAsync();
                            //    }
                            //    catch (Exception exc)
                            //    {
                            //        // TODO exception
                            //        throw;
                            //    }
                            //}
                            
                        }
                    }
                    catch (Exception exc)
                    {
                        _errorOccurred = true;
#if WINDOWS_UWP
                        TrackingManager.TrackExceptionCatched(nameof(CheckLocalDatafile) + " PW", exc);
#endif
                        await ErrorDialogs.ShowPasswordError();
                        //CheckLocalDatafile();
                        
                    }
                    finally
                    {
                        IsLoading = false;
#if __IOS__
                        if (nsUrl != null)
                        {
                            // release the access of the file
                            nsUrl.StopAccessingSecurityScopedResource();
                        }
#endif
                    }

                }
                // file not found case
                else
                {
                    if (dbDatafile.IsWebDAV)
                    {
                        // TODO show not found error
                        await ErrorDialogs.ShowFileOrFolderNotFoundError(dbDatafile);
                        //await CheckLocalDatafile();
                    }
                    else
                    {
                        _errorOccurred = true;
                        await ErrorDialogs.ShowFileOrFolderNotFoundError();
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _errorOccurred = true;
                if (exc is UnauthorizedAccessException)
                {
                    await ErrorDialogs.ShowUnauthorizedAccessError();
                }
                else if(exc is FileNotFoundException)
                {
                    if (dbDatafile.IsWebDAV)
                    {
#if WINDOWS_UWP
                        TrackingManager.TrackExceptionCatched(nameof(CheckLocalDatafile) + " WebD", exc);
#endif
                        // TODO add dialog for error
                        ///var webDAVTask = await CheckIfWebDAVDatafileIsOutdated(dbDatafile);
                    }
                    else
                    {
                        await ErrorDialogs.ShowFileOrFolderNotFoundError();
                    }
                }
                else
                {
#if WINDOWS_UWP
                    TrackingManager.TrackException(nameof(CheckLocalDatafile),exc);
#endif
                    await ErrorDialogs.ShowUnexpectedError(exc);
                }
            }
            finally
            {
                // writing status for the data file is activated again
                _initialization = false;
                CollectionAccessSemaphore.Release();
            }

            //check if a newer version is available or the current file must be uploaded
            if (dbDatafile != null && dbDatafile.IsWebDAV && ActivatedDatafile == null)
            {
                if (await NetworkService.GetIsInternetAvailableAsync())
                {
                    var (success, outdated) = await CheckIfWebDAVDatafileIsEqual(dbDatafile);
                    if (success && outdated)
                    {
                        Collection.Clear();
                        await CheckLocalDatafile(); //reload the data file
                        Messenger.Send(new WebDAVStatusChangedMessage(WebDAVStatus.Updated));
                    }
                    else if (!success)
                    {
                        // TODO add dialog for error, the path of the file is changed
                    }
                }
                else
                {
                    Messenger.Send(new WebDAVStatusChangedMessage(WebDAVStatus.NoInternet));
                }
            }
        }

        //private async void Query_DatafileChanged(IStorageQueryResultBase sender, object args)
        //{
        //    if (_queryChangedCounter != 0)
        //    {
        //        _initialization = true;
        //        // reload the datafile only, when the file is modified outside the app
        //        if (!_datafileWritten)
        //        {
        //            // TODO display information for reloading
        //            // reload the datafile, if the file is changed
        //            await ReloadDatafile();
        //        }
        //        else
        //        {
        //            _datafileWritten = false;
        //        }
        //    }
        //    _queryChangedCounter++;
        //}

        public void ErrorResolved()
        {
            _errorOccurred = false;
        }

        private void SendPasswordStatusMessage(bool isinvalid, string hash)
        {
            Messenger.Send(new PasswordStatusChangedMessage(isinvalid, hash));
        }


        /// <summary>
        /// Writes the current accounts into the datafile
        /// </summary>
        public async Task WriteLocalDatafile()
        {
            string fileName = string.Empty;
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
#if !WINDOWS_UWP
            StorageFile file = null;
#endif
#if __IOS__
            Foundation.NSUrl nsUrl = null;
#endif
            DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
            DBDatafileModel datafileDB = await App.Repository.Datafile.GetAsync();
            // load the current file to allow the reset of the file
            DatafileModel datafile = new DatafileModel();
            try
            {
                await CollectionAccessSemaphore.WaitAsync();

                byte[] iv = Aes.Create().IV;
                string passwordHashName = ActivatedDatafile != null ?
                    Constants.ActivatedDatafileHashName :
                    dbHash.Hash;

                // set new template version for data file
#if WINDOWS_UWP
                var prevíousVersion = new Version(Microsoft.Toolkit.Uwp.Helpers.SystemInformation.Instance.ApplicationVersion.ToFormattedString());
#else
                // TODO Uno release
                var prevíousVersion = new Version("1.3.0.0");
#endif

                var compareVersion = new Version("1.3.0.0");
                var result = prevíousVersion.CompareTo(compareVersion);
                int version = result >= 0 ? 2 : 1;
                // create the new datafile model
                DatafileModel fileModel = new DatafileModel() { IV = iv, Collection = Collection, Version = version, GlobalCategories = GlobalCategories };
                
                if (ActivatedDatafile != null)
                {
#if WINDOWS_UWP
                    folder = await ActivatedDatafile.GetParentAsync();
#endif
                    fileName = ActivatedDatafile.Name;
                }
                else
                {
#if WINDOWS_UWP
                    folder = datafileDB.IsWebDAV ?
                    ApplicationData.Current.LocalFolder :
                    await StorageFolder.GetFolderFromPathAsync(datafileDB.Path);
#endif
                    fileName = datafileDB.Name;
#if __ANDROID__
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
                    await Task.Run(async () => {
                        Android.Net.Uri androidUri = Android.Net.Uri.Parse(datafileDB.Path);
                        file = StorageFile.GetFromSafUri(androidUri);
                    });
#endif
#if __IOS__
                    Foundation.NSData bookmark = Foundation.NSUserDefaults.StandardUserDefaults.DataForKey(Constants.ContainerName);
                    nsUrl = Foundation.NSUrl.FromBookmarkData(
                        bookmark,
                        Foundation.NSUrlBookmarkResolutionOptions.WithSecurityScope,
                        null,
                        out bool isStale,
                        out Foundation.NSError error);
                    file = await StorageFile.GetFileFromPathAsync(nsUrl.Path);
#endif
                }

                // backup the last version before write
                string datafileStr = await FileService.ReadStringAsync(fileName, folder);
                datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);


                if (ActivatedDatafile != null)
                {
#if WINDOWS_UWP
                    await FileService.WriteStringAsync(
                        fileName,
                        NewtonsoftJSONService.SerializeEncrypt(
                            ProtectData.Unprotect(NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName))),
                            iv,
                            fileModel,
                            fileModel.Version),
                        folder);
#else
                    string content = NewtonsoftJSONService.SerializeEncrypt(
                            NewtonsoftJSONService.Deserialize<byte[]>(SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName)),
                            iv,
                            fileModel,
                            fileModel.Version);
#if __ANDROID__
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
                    await Task.Run(async () => {
                        await FileIO.WriteTextAsync(ActivatedDatafile, content);
                    });
#else
                    await FileIO.WriteTextAsync(ActivatedDatafile, content);
#endif

#endif
                }
                else
                {
#if WINDOWS_UWP
                    await FileService.WriteStringAsync(
                        fileName,
                        NewtonsoftJSONService.SerializeEncrypt(
                            SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName), 
                            iv, 
                            fileModel,
                            fileModel.Version),
                        folder);
#else
#if __IOS__
                    if (!Foundation.NSFileManager.DefaultManager.IsWritableFile(nsUrl.Path))
                    {
                        nsUrl.StartAccessingSecurityScopedResource();
                    }
#endif
                    //var fileStream = await file.OpenStreamForWriteAsync();
                    string content = NewtonsoftJSONService.SerializeEncrypt(
                            SecretService.Helper.ReadSecret(Constants.ContainerName, passwordHashName),
                            iv,
                            fileModel,
                            fileModel.Version);
#if __ANDROID__
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
                    await Task.Run(async () => {
                        await FileIO.WriteTextAsync(file, content);
                    });
#else
                    await FileIO.WriteTextAsync(file, content);
#endif

                    //using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
                    //{
                    //    await writer.WriteAsync(content);
                    //}


                    //await fileStream.WriteAsync(Encoding.UTF8.GetBytes(content));
                    //fileStream.Close();
#endif

                }


                if (datafileDB != null && datafileDB.IsWebDAV && ActivatedDatafile == null)
                {
                    // TODO check result
                    (bool successful, bool statusResult) = await UploadDatafileWithWebDAV(folder, datafileDB);
                    if (successful && statusResult)
                    {

                    }
                    else
                    {

                    }
                }
                else
                {

                }
                Messenger.Send(new DatafileWriteStatusChangedMessage(true));
                CollectionAccessSemaphore.Release();
            }
            catch (Exception exc)
            {
                CollectionAccessSemaphore.Release();
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(WriteLocalDatafile), exc);
#endif
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                HandleWriteError(datafile,fileName,folder);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            }
        }

        private async Task HandleWriteError(DatafileModel datafile, string fileName, StorageFolder folder, int count = 0)
        {
            var restoreSuccess = await RestoreLastDatafile(datafile, fileName, folder);
            if (restoreSuccess)
            {
                await Utils.ErrorDialogs.WritingDatafileError(false);
            }
            else
            {
                if (count < 3)
                {
                    await Task.Delay(250);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    HandleWriteError(datafile, fileName, folder, count + 1);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    await Utils.ErrorDialogs.WritingFatalRestoreError();
                }
            }
        }

        private async Task<bool> RestoreLastDatafile(DatafileModel datafile, string fileName, StorageFolder folder)
        {
            try
            {
                await CollectionAccessSemaphore.WaitAsync();
                await FileService.WriteStringAsync(
                    fileName,
                    NewtonsoftJSONService.Serialize(datafile),
                    folder);
                return true;
            }
            catch (Exception exc)
            {
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(RestoreLastDatafile), exc);
#endif
                return false;
            }
            finally
            {
                CollectionAccessSemaphore.Release();
            }
            

        }

        /// <summary>
        /// Generates a TOTP code for the i'th entry of a collection
        /// </summary>
        /// <param name="i"></param>
        public async Task GenerateTOTP(int i)
        {
            try
            {
                if (Collection[i].SecretByteArray != null)
                {
                    Totp totp = new Totp(Collection[i].SecretByteArray, Collection[i].Period, Collection[i].HashMode, Collection[i].TotpSize);
                    int remainingTime;

                    if (_checkedTimeSynchronisation && _ntpServerTimeDifference != null)
                    {
                        Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow.AddMilliseconds(_ntpServerTimeDifference.TotalMilliseconds));
                        //calc the remaining time for the TOTP code with the time correction
                        remainingTime = Collection[i].Period -
                            (int)((DateTime.UtcNow.AddMilliseconds(_ntpServerTimeDifference.TotalMilliseconds).Ticks - unixEpochTicks)
                            / ticksToSeconds % Collection[i].Period);
                    }
                    else
                    {
                        Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
                        remainingTime = totp.RemainingSeconds();
                    }
                    Logger.Log("TOTP remaining Time: " + remainingTime.ToString(), Category.Debug, Priority.None);
                    Collection[i].Seconds = remainingTime;
                }
                else
                {
                    throw new ArgumentNullException(Collection[i].Label);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.High);
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(GenerateTOTP), ex);
#endif
                _reloadCollectionCounter++;
                if (_reloadCollectionCounter < 3)
                {
                    await ReloadDatafile();
                }
                else
                {
                    // track if the creation finally failed
#if WINDOWS_UWP
                    TrackingManager.TrackExceptionUnhandled(nameof(GenerateTOTP), ex);
#endif
                    Collection[i].TwoFACode = string.Empty;
                    await ErrorDialogs.SecretKeyError(Collection[i].Label).ConfigureAwait(false);
                }
            }
        }

        public async Task ReloadAccountIconSVGs()
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                await SVGColorHelper.GetSVGIconWithThemeColor(Collection[i], Collection[i].AccountIconName);
            }
        }



#region WebDAV
        /// <summary>
        /// Upload the data file with custom WebDAV header
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="datafileDB"></param>
        /// <returns></returns>
        public async Task<(bool successful, bool statusResult)> UploadDatafileWithWebDAV(StorageFolder folder, DBDatafileModel datafileDB, bool initial=false)
        {
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
            if (await FileService.FileExistsAsync(datafileDB.Name, folder))
            {
                StorageFile file = await folder.GetFileAsync(datafileDB.Name);
                CancellationTokenSource cts = new CancellationTokenSource();
                //IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();
                if (initial || await client.ExistsAsync(datafileDB.Path + "/" + datafileDB.Name))
                {
                    //set custom date properties (current file date) to prevent that a newer date with the upload is created
                    string modifiedDate = (await file.GetBasicPropertiesAsync()).DateModified.ToUniversalTime().ToUnixTimeSeconds().ToString();
                    List<(string Key, string Value)> headerPairs = new List<(string Key, string Value)>(); //custom http headers (owncloud and nextcloud)
                    headerPairs.Add(("X-OC-MTime", modifiedDate)); //last modified date
                    headerPairs.Add(("X-OC-CTime", file.DateCreated.ToUniversalTime().ToUnixTimeSeconds().ToString())); //creation date
                    var uploaded = await client.UploadAsync(
                        datafileDB.Path + "/" + datafileDB.Name,
                        await file.OpenStreamForReadAsync(),
                        file.ContentType,
                        cts.Token,
                        headerPairs);
                    if (uploaded)
                    {
                        // TODO webdav file upload failed
                        return (true, true);
                    }
                    else
                    {
                        return (false, false);
                    }
                }
                else
                {
                    // TODO webdav file was moved
                    return (false, false);
                }
            }
            else
            {
                return (false, false);
            }

        }

        /// <summary>
        /// Download the datafile in the local app storage
        /// </summary>
        /// <param name="storageFolder"></param>
        /// <param name="dbDatafile"></param>
        /// <returns></returns>
        private async Task<StorageFile> DownloadWebDAVFile(StorageFolder storageFolder, DBDatafileModel dbDatafile)
        {
            //todo create date is correct?
            StorageFile localFile;
            localFile = await storageFolder.CreateFileAsync(dbDatafile.Name, CreationCollisionOption.ReplaceExisting);
            IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
            using IRandomAccessStream randomAccessStream = await localFile.OpenAsync(FileAccessMode.ReadWrite);
            Stream targetStream = randomAccessStream.AsStreamForWrite();
            await client.Download(dbDatafile.Path + "/" + dbDatafile.Name, targetStream, progress, new CancellationToken());
            return localFile;
        }

        /// <summary>
        /// Check if the datafile is outdated
        /// </summary>
        /// <param name="dbDatafile"></param>
        private async Task<(bool successful, bool outdated)> CheckIfWebDAVDatafileIsEqual(DBDatafileModel dbDatafile)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await storageFolder.GetFileAsync(dbDatafile.Name);
                WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
                ResourceInfoModel webDAVFile = await client.GetResourceInfoAsync(dbDatafile.Path, dbDatafile.Name);

                DateTime fileDateModified = (await storageFile.GetBasicPropertiesAsync()).DateModified.UtcDateTime;
                DateTime trimmedFileLastModified = new DateTime(fileDateModified.Year, fileDateModified.Month,
                    fileDateModified.Day, fileDateModified.TimeOfDay.Hours,
                    fileDateModified.TimeOfDay.Minutes, fileDateModified.TimeOfDay.Seconds);

                var webDAVDateModified = webDAVFile != null? webDAVFile.LastModified.ToUniversalTime() : trimmedFileLastModified;
                if (webDAVDateModified > trimmedFileLastModified && webDAVFile != null)
                {
                    await DownloadWebDAVFile(storageFolder, dbDatafile);
                    return (true, true);
                }
                else if (webDAVDateModified < trimmedFileLastModified || webDAVFile == null)
                {
                    DBDatafileModel datafileDB = await App.Repository.Datafile.GetAsync();
                    if (webDAVFile == null)
                    {
                        (bool successful, bool statusResult) = await UploadDatafileWithWebDAV(storageFolder, datafileDB, true);
                        return (successful, false);
                    }
                    else
                    {
                        (bool successful, bool statusResult) = await UploadDatafileWithWebDAV(storageFolder, datafileDB);
                        return (successful, false);
                    }
                }
                else
                {
                    return (true, false);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.High);
                return (false, false);
            }
        }
#endregion


#region GetSet
        public TwoFACodeModel TempDeletedTFAModel
        {
            get => _tempDeletedTFAModel;
            set
            {
                SetProperty(ref _tempDeletedTFAModel, value);
            }
        }

        public bool RestoreDeletedModel()
        {
            if (_tempDeletedTFAModel != null)
            {
                Collection.Add(_tempDeletedTFAModel);
                TempDeletedTFAModel = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// If collection is empty, bool is true => open teaching tip to add a new account
        /// </summary>
        public bool EmptyAccountCollectionTipIsOpen
        {
            get => _emptyAccountCollectionTipIsOpen;
            set => SetProperty(ref _emptyAccountCollectionTipIsOpen, value);
        }

        public StorageFile ActivatedDatafile 
        { 
            get => _openDatefile;
            set => SetProperty(ref _openDatefile, value);
        }

        public bool IsLoading 
        { 
            get => _isLoading; 
            set => SetProperty(ref _isLoading, value); 
        }
        public bool IsFirstActivatedDatafileStart { get => _isFirstActivatedDatafileStart; set => _isFirstActivatedDatafileStart = value; }
#if __IOS__
        public Foundation.NSUrl OpenDatefileUrl { get => _openDatefileUrl; set => _openDatefileUrl = value; }
#endif
#endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _collectionAccessSemaphore?.Dispose();
            }
        }
    }
}