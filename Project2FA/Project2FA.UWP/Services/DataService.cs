using Prism.Mvvm;
using Project2FA.Repository.Models;
using System;
using System.Collections.ObjectModel;
using Prism.Ioc;
using Template10.Services.Secrets;
using Template10.Services.File;
using Project2FA.Core.Services.JSON;
using Windows.Storage;
using System.Collections.Specialized;
using OtpNet;
using Windows.UI.Xaml.Controls;
using Prism.Commands;
using Prism.Logging;
using Project2FA.Core.Utils;
using Windows.UI.Xaml;
using System.Threading;
using System.Security.Cryptography;
using Project2FA.UWP.Views;
using Project2FA.UWP.Strings;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.ApplicationModel.Core;
using System.IO;
using Microsoft.Toolkit.Uwp.UI;
using Project2FA.UWP.Utils;
using Microsoft.Toolkit.Uwp.Connectivity;
using Project2FA.Core;
using Project2FA.Core.Services.NTP;
using System.Threading.Tasks;
using DecaTec.WebDav;
using Project2FA.UWP.Services.WebDAV;
using Windows.Storage.Streams;
using WebDAVClient.Types;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Project2FA.UWP.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Diagnostics;

namespace Project2FA.UWP.Services
{
    public class DataService : BindableBase, IDisposable
    {
        public SemaphoreSlim CollectionAccessSemaphore { get; } = new SemaphoreSlim(1, 1);
        private bool _checkedTimeSynchronisation;
        private TimeSpan _ntpServerTimeDifference;
        private ISecretService SecretService { get; }
        private IDialogService DialogService { get; }
        private ILoggerFacade Logger { get; }
        private IFileService FileService { get; }
        private INetworkTimeService NetworkTimeService { get; }
        private bool _initialization, _errorOccurred;
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        public Stopwatch TOTPEventStopwatch { get; }
        public AdvancedCollectionView ACVCollection { get; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();
        private bool _emptyAccountCollectionTipIsOpen;
        private TwoFACodeModel _tempDeletedTFAModel;
        private const long unixEpochTicks = 621355968000000000L;
        private const long ticksToSeconds = 10000000L;

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

            //SystemInformation.Instance.PreviousVersionInstalled >= PackageVersionHelper.ToPackageVersion("1.0.9.0")
            if (SystemInformation.Instance.IsAppUpdated) //SystemInformation.Instance.IsAppUpdated && 
            {
                var prevíousVersion = new Version(SystemInformation.Instance.PreviousVersionInstalled.ToFormattedString());
                var compareVersion = new Version("1.0.9.0");
                var result = compareVersion.CompareTo(prevíousVersion);
                if (result >= 0)
                {
                    string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                    string path = root + @"\Assets\AccountIcons";
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    // try and set the account icon name
                    for (int i = 0; i < Collection.Count; i++)
                    {
                        string label = Collection[i].Label;
                        label = label.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty);
                        if (await FileService.FileExistsAsync(string.Format("{0}.svg", label), folder))
                        {
                            Collection[i].AccountIconName = label;
                            await SVGColorHelper.GetSVGIconWithThemeColor(Collection[i], label);
                        }
                    }
                    await WriteLocalDatafile();
                }
            }
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
                        Logger.Log("NTP exception: " + exc.Message, Category.Exception, Priority.High);
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
                            var iconStr = await SVGColorHelper.GetSVGIconWithThemeColor(Collection[i], Collection[i].AccountIconName);
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
        /// <param name="dbDatafile"></param>
        private async Task CheckLocalDatafile()
        {
            DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
            try
            {
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();
                StorageFolder folder = dbDatafile.IsWebDAV ? 
                    ApplicationData.Current.LocalFolder : 
                    await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
                if (await FileService.FileExistsAsync(dbDatafile.Name, folder))
                {
                    DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
                    // prevent write of the datafile to folder
                    _initialization = true;
                    try
                    {
                        string datafileStr = await FileService.ReadStringAsync(dbDatafile.Name, folder);
                        if (!string.IsNullOrEmpty(datafileStr))
                        {
                            // read the iv for AES
                            DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            byte[] iv = datafile.IV;

                            datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                                            (SecretService.Helper.ReadSecret(Constants.ContainerName,dbHash.Hash),
                                            iv,
                                            datafileStr);
                            deserializeCollection = datafile.Collection;
                        }
                        if (deserializeCollection != null)
                        {
                            await CollectionAccessSemaphore.WaitAsync();
                            Collection.AddRange(deserializeCollection);
                            if (Collection.Count == 0)
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
                            CollectionAccessSemaphore.Release();
                        }
                    }
                    catch (Exception)
                    {
                        _errorOccurred = true;
                        await ErrorDialogs.ShowPasswordError();
                    }
                }
                // file not found case
                else
                {
                    if (dbDatafile.IsWebDAV)
                    {
                        //var webDAVTask = await CheckIfWebDAVDatafileIsEqual(dbDatafile);
                        //await CheckLocalDatafile();
                    }
                    else
                    {
                        _errorOccurred = true;
                        await ShowFileOrFolderNotFoundError();
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
                        // TODO add dialog for error
                        ///var webDAVTask = await CheckIfWebDAVDatafileIsOutdated(dbDatafile);
                    }
                    else
                    {
                        await ShowFileOrFolderNotFoundError();
                    }
                }
                else
                {
                    TrackingManager.TrackException(exc);
                    await ErrorDialogs.ShowUnexpectedError(exc);
                }
            }
            //check if a newer version is available or the current file must be uploaded
            if (dbDatafile.IsWebDAV)
            {
                var (success, outdated) = await CheckIfWebDAVDatafileIsEqual(dbDatafile);
                if (success && outdated)
                {
                    Collection.Clear();
                    await CheckLocalDatafile(); //reload the data file
                    // TODO Add info message to inform that the file was updated
                }
                else if (!success)
                {
                    // TODO add dialog for error, the path of the file is changed
                }

            }
            // writing status for the data file is activated again
            _initialization = false;
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
                var fileDateModified = (await storageFile.GetBasicPropertiesAsync()).DateModified.UtcDateTime;
                DateTime trimmedFileLastModified = new DateTime(fileDateModified.Year, fileDateModified.Month, 
                    fileDateModified.Day, fileDateModified.TimeOfDay.Hours,
                    fileDateModified.TimeOfDay.Minutes, fileDateModified.TimeOfDay.Seconds);
                var webDAVDateModified = webDAVFile.LastModified.ToUniversalTime();
                if (webDAVDateModified > trimmedFileLastModified)
                {
                    await DownloadWebDAVFile(storageFolder, dbDatafile);
                    return (true, true);
                }
                else if (webDAVDateModified < trimmedFileLastModified)
                {
                    DBDatafileModel datafileDB = await App.Repository.Datafile.GetAsync();
                    // TODO check result
                    (bool successful, bool statusResult) = await UploadDatafileWithWebDAV(storageFolder, datafileDB);
                    return (true, false);
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

        private void ErrorResolved()
        {
            _errorOccurred = false;
            // allow shell navigation
            App.ShellPageInstance.NavigationIsAllowed = true;
        }

        /// <summary>
        /// Displays a FileNotFoundException message and the option for factory reset or correcting the path
        /// </summary>
        public async Task ShowFileOrFolderNotFoundError()
        {
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            }
            catch (UnauthorizedAccessException)
            {
                await ErrorDialogs.UnauthorizedAccessDialog();
            }
            // disable shell navigation
            App.ShellPageInstance.NavigationIsAllowed = false;
            //Logger.Log("no datafile found", Category.Exception, Priority.High);
            bool selectedOption = false;

            ContentDialog dialog = new ContentDialog();
            dialog.Closed += Dialog_Closed;
            dialog.Title = Resources.ErrorHandle;
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.ExceptionDatafileNotFound;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(markdown);

            Button changePathBTN = new Button();
            changePathBTN.Margin = new Thickness(0, 10, 0, 0);
            changePathBTN.Content = Resources.ChangeDatafilePath;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            changePathBTN.Command = new DelegateCommand(async () =>
            {
                selectedOption = true;
                dialog.Hide();
                ContentDialogResult result = await DialogService.ShowDialogAsync(new UpdateDatafileContentDialog(), new DialogParameters());
                if (result == ContentDialogResult.Primary)
                {
                    ErrorResolved();
                    await CheckLocalDatafile();
                }
                if (result == ContentDialogResult.None)
                {
                    await ShowFileOrFolderNotFoundError();
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            stackPanel.Children.Add(changePathBTN);

            Button factoryResetBTN = new Button();
            factoryResetBTN.Margin = new Thickness(0, 10, 0, 10);
            factoryResetBTN.Content = Resources.FactoryReset;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            factoryResetBTN.Command = new DelegateCommand(async () =>
            {
                DBPasswordHashModel passwordHash = await App.Repository.Password.GetAsync();
                //delete password in the secret vault
                SecretService.Helper.RemoveSecret(Constants.ContainerName, passwordHash.Hash);
                // reset data and restart app
                await ApplicationData.Current.ClearAsync();
                await CoreApplication.RequestRestartAsync("Factory reset");
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            stackPanel.Children.Add(factoryResetBTN);

            dialog.Content = stackPanel;
            dialog.PrimaryButtonText = Resources.CloseApp;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.PrimaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });

            async void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
            {
                if (!(Window.Current.Content is ShellPage))
                {
                    Prism.PrismApplicationBase.Current.Exit();
                }
                else
                {
                    if (!selectedOption)
                    {
                        await DialogService.ShowDialogAsync(dialog, new DialogParameters());
                    }
                }
            }
            await DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        /// <summary>
        /// Writes the current accounts into the datafile
        /// </summary>
        public async Task WriteLocalDatafile()
        {
            DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
            DBDatafileModel datafileDB = await App.Repository.Datafile.GetAsync();
            byte[] iv = new AesManaged().IV;

            await CollectionAccessSemaphore.WaitAsync();
            DatafileModel fileModel = new DatafileModel() { IV = iv, Collection = Collection };
            StorageFolder folder = datafileDB.IsWebDAV ?
                ApplicationData.Current.LocalFolder :
                await StorageFolder.GetFolderFromPathAsync(datafileDB.Path);

            await FileService.WriteStringAsync(
                    datafileDB.Name,
                    NewtonsoftJSONService.SerializeEncrypt(SecretService.Helper.ReadSecret(Constants.ContainerName, dbHash.Hash),
                    iv, 
                    fileModel),
                    folder);

            if (datafileDB.IsWebDAV)
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
            CollectionAccessSemaphore.Release();
        }
        
        /// <summary>
        /// Upload the data file with custom WebDAV header
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="datafileDB"></param>
        /// <returns></returns>
        private async Task<(bool successful, bool statusResult)> UploadDatafileWithWebDAV(StorageFolder folder, DBDatafileModel datafileDB)
        {
            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
            StorageFile file = await folder.GetFileAsync(datafileDB.Name);
            CancellationTokenSource cts = new CancellationTokenSource();
            //IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();
            if (await client.ExistsAsync(datafileDB.Path + "/" + datafileDB.Name))
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
        /// Generates a TOTP code for the i'th entry of a collection
        /// </summary>
        /// <param name="i"></param>
        public async Task GenerateTOTP(int i)
        {
            try
            {
                if (Collection[i].SecretByteArray is null)
                {
                    TrackingManager.TrackEvent(Category.Warn, Priority.High, "Secret key is empty!");
                    await ReloadDatafile();
                }
                else
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
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.High);
                TrackingManager.TrackExceptionCatched(ex);
            }
        }

        public async Task ReloadAccountIconSVGs()
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                var iconStr = await SVGColorHelper.GetSVGIconWithThemeColor(Collection[i], Collection[i].AccountIconName);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CollectionAccessSemaphore?.Dispose();
            }
        }
    }
}
