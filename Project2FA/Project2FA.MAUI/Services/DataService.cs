using OtpNet;
using Project2FA.Core.Utils;
using Project2FA.Core.Services.JSON;
using Project2FA.Core.Services.NTP;
using Project2FA.MAUI.Helpers;
using Project2FA.Repository.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Project2FA.MAUI.Services
{
    public class DataService : ObservableObject, IDisposable
    {
        public SemaphoreSlim CollectionAccessSemaphore { get; } = new SemaphoreSlim(1, 1);
        private bool _checkedTimeSynchronisation;
        private TimeSpan _ntpServerTimeDifference;
        //private IDialogService DialogService { get; }
        //private ILoggerFacade Logger { get; }
        private INetworkTimeService NetworkTimeService { get; }
        private bool _initialization, _errorOccurred;
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        public Stopwatch TOTPEventStopwatch { get; }

        private string _storageFileUrl;
        private string _storageFileContent;
        private string _filePasswordHash;

        private static readonly ObservableCollection<TwoFACodeModel> twoFACodeModels = new ObservableCollection<TwoFACodeModel>();

        //public AdvancedCollectionView ACVCollection { get; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = twoFACodeModels;
        private bool _emptyAccountCollectionTipIsOpen;
        private TwoFACodeModel _tempDeletedTFAModel;
        private const long unixEpochTicks = 621355968000000000L;
        private const long ticksToSeconds = 10000000L;
        int _reloadCollectionCounter = 0;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static DataService Instance { get; } = new DataService();

        /// <summary>
        /// Private Constructor
        /// </summary>
        private DataService()
        {
            //Logger = App.Current.Container.Resolve<ILoggerFacade>();
            //DialogService = App.Current.Container.Resolve<IDialogService>();
            NewtonsoftJSONService = new NewtonsoftJSONService();
            NetworkTimeService = new NetworkTimeService();
            //ACVCollection = new AdvancedCollectionView(Collection, true);
            TOTPEventStopwatch = new Stopwatch();
            //ACVCollection.SortDescriptions.Add(new SortDescription("Label", SortDirection.Ascending));
            //ACVCollection.SortDescriptions.Add(new SortDescription("IsFavouriteText", SortDirection.Ascending));
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
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && SettingsService.Instance.UseNTPServerCorrection)
                {
                    try
                    {
                        DateTime time = await NetworkTimeService.GetNetworkTimeAsync(SettingsService.Instance.NTPServerString);
                        TimeSpan timespan = time.Subtract(DateTime.UtcNow);
                        _checkedTimeSynchronisation = true;
                        if (Math.Abs(timespan.TotalSeconds) >= 15) // difference of 15 seconds or more
                        {
                            _ntpServerTimeDifference = timespan;
                            //await ErrorDialogs.SystemTimeNotCorrectError();
                        }
                        SettingsService.Instance.LastCheckedSystemTime = DateTime.UtcNow;
                    }
                    catch (Exception exc)
                    {
                        //Logger.Log("NTP exception: " + exc.Message, Category.Exception, Priority.Low);
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
            DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
            try
            {
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();
                //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
                //FileResult fileResult;
                //try
                //{
                //    fileResult = new FileResult(dbDatafile.Path);
                //}
                //catch (Exception)
                //{

                //    throw;
                //}
                
                if(true) //if (File.Exists(dbDatafile.Path))
                {
                    DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
                    //FileHelper fileHelper = new FileHelper();
                    // prevent write of the datafile to folder
                    _initialization = true;
                    try
                    {
                        //string datafileStr;
                        //FileResult fileResult = new FileResult(dbDatafile.Path);
                        //using (StreamReader reader = new StreamReader(await fileResult.OpenReadAsync()))
                        //{
                        //    datafileStr = reader.ReadToEnd();

                        //}

                        //fileHelper.StartAccessFile(dbDatafile.Path);
                        string path = string.IsNullOrWhiteSpace(FilePasswordHash) ? dbDatafile.Path : StorageFileUrl;
                        string hash = string.IsNullOrWhiteSpace(FilePasswordHash) ? dbHash.Hash : FilePasswordHash;
                        string datafileStr = !string.IsNullOrWhiteSpace(path) ? await File.ReadAllTextAsync(path): StorageFileContent;
                        if (!string.IsNullOrEmpty(datafileStr))
                        {
                            // read the iv for AES
                            DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            byte[] iv = datafile.IV;

                            datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                                            (await SecureStorage.Default.GetAsync(hash),
                                            iv,
                                            datafileStr);
                            deserializeCollection = datafile.Collection;
                        }
                        if (deserializeCollection != null)
                        {
                            await CollectionAccessSemaphore.WaitAsync();
                            if (Collection.AddRange(deserializeCollection,true) == 0)
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
                    catch (Exception exc)
                    {
                        _errorOccurred = true;
                        //await ErrorDialogs.ShowPasswordError();
                    }
                    finally
                    {
                        //fileHelper.StopAccessFile(dbDatafile.Path);
                    }
                }
                // file not found case
                else
                {
                    _errorOccurred = true;
                    //await ShowFileOrFolderNotFoundError();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _errorOccurred = true;
                if (exc is UnauthorizedAccessException)
                {
                    //await ErrorDialogs.ShowUnauthorizedAccessError();
                }
                else if (exc is FileNotFoundException)
                {
                    //await ShowFileOrFolderNotFoundError();
                }
                else
                {
                    //TrackingManager.TrackException(exc);
                    //await ErrorDialogs.ShowUnexpectedError(exc);
                }
            }
            // writing status for the data file is activated again
            _initialization = false;
        }

        private void ErrorResolved()
        {
            _errorOccurred = false;
            // allow shell navigation
            //App.ShellPageInstance.NavigationIsAllowed = true;
        }

        /// <summary>
        /// Displays a FileNotFoundException message and the option for factory reset or correcting the path
        /// </summary>
//        public async Task ShowFileOrFolderNotFoundError()
//        {
//            //try
//            //{
//            //    //TODO current workaround: check permission to the file system (broadFileSystemAccess)
//            //    string path = @"C:\Windows\explorer.exe";
//            //    StorageFile file = await StorageFile.GetFileFromPathAsync(path);
//            //}
//            //catch (UnauthorizedAccessException)
//            //{
//            //    await ErrorDialogs.UnauthorizedAccessDialog();
//            //}


//            // disable shell navigation
//            //App.ShellPageInstance.NavigationIsAllowed = false;
//            //Logger.Log("no datafile found", Category.Exception, Priority.High);
//            bool selectedOption = false;

//            ContentDialog dialog = new ContentDialog();
//            dialog.Closed += Dialog_Closed;
//            dialog.Title = Resources.ErrorHandle;
//            MarkdownTextBlock markdown = new MarkdownTextBlock();
//            markdown.Text = Resources.ExceptionDatafileNotFound;
//            StackPanel stackPanel = new StackPanel();
//            stackPanel.Children.Add(markdown);

//            Button changePathBTN = new Button();
//            changePathBTN.Margin = new Thickness(0, 10, 0, 0);
//            changePathBTN.Content = Resources.ChangeDatafilePath;

//#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
//            changePathBTN.Command = new DelegateCommand(async () =>
//            {
//                selectedOption = true;
//                dialog.Hide();
//                ContentDialogResult result = await DialogService.ShowDialogAsync(new UpdateDatafileContentDialog(), new DialogParameters());
//                if (result == ContentDialogResult.Primary)
//                {
//                    ErrorResolved();
//                    await CheckLocalDatafile();
//                }
//                if (result == ContentDialogResult.None)
//                {
//                    await ShowFileOrFolderNotFoundError();
//                }
//            });
//#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
//            stackPanel.Children.Add(changePathBTN);

//            Button factoryResetBTN = new Button();
//            factoryResetBTN.Margin = new Thickness(0, 10, 0, 10);
//            factoryResetBTN.Content = Resources.FactoryReset;

//#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
//            factoryResetBTN.Command = new DelegateCommand(async () =>
//            {
//                DBPasswordHashModel passwordHash = await App.Repository.Password.GetAsync();
//                //delete password in the secret vault
//                SecureStorage.Default.RemoveAll();
//                //SecureStorage.Default.Remove(passwordHash.Hash);
//                // reset data and restart app
//                await ApplicationData.Current.ClearAsync();
//                await CoreApplication.RequestRestartAsync("Factory reset");
//            });
//#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

//            stackPanel.Children.Add(factoryResetBTN);

//            dialog.Content = stackPanel;
//            dialog.PrimaryButtonText = Resources.CloseApp;
//            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
//            dialog.PrimaryButtonCommand = new DelegateCommand(() =>
//            {
//                Prism.PrismApplicationBase.Current.Exit();
//            });

//            async void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
//            {
//                if (!(App.Current.MainPage is AppShell))
//                {
//                    App.Current.Quit();
//                }
//                else
//                {
//                    if (!selectedOption)
//                    {
//                        await DialogService.ShowDialogAsync(dialog, new DialogParameters());
//                    }
//                }
//            }
//            await DialogService.ShowDialogAsync(dialog, new DialogParameters());
//        }

        /// <summary>
        /// Writes the current accounts into the datafile
        /// </summary>
        public async Task WriteLocalDatafile()
        {
            DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
            DBDatafileModel datafileDB = await App.Repository.Datafile.GetAsync();
            Aes aes = Aes.Create();
            byte[] iv = aes.IV;

            await CollectionAccessSemaphore.WaitAsync();
            DatafileModel fileModel = new DatafileModel() { IV = iv, Collection = Collection };
            //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(datafileDB.Path);

            
            //var filestream = File.OpenWrite(datafileDB.Path);
            //filestream.
            //await FileService.WriteStringAsync(
            //        datafileDB.Name,
            //        NewtonsoftJSONService.SerializeEncrypt(await SecureStorage.Default.GetAsync(dbHash.Hash),
            //        iv,
            //        fileModel),
            //        folder);

            CollectionAccessSemaphore.Release();
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
                    //Logger.Log("TOTP remaining Time: " + remainingTime.ToString(), Category.Debug, Priority.None);
                    Collection[i].Seconds = remainingTime;
                }
                else
                {
                    //TODO  add dialog
                }

            }
            catch (Exception ex)
            {
                //Logger.Log(ex.Message, Category.Exception, Priority.High);
                //TrackingManager.TrackException(ex);
                _reloadCollectionCounter++;
                if (_reloadCollectionCounter < 3)
                {
                    await ReloadDatafile();
                }
                else
                {
                    // TODO add dialog
                    throw;
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

        /// <summary>
        /// If collection is empty, bool is true => open teaching tip to add a new account
        /// </summary>
        public bool EmptyAccountCollectionTipIsOpen
        {
            get => _emptyAccountCollectionTipIsOpen;
            set => SetProperty(ref _emptyAccountCollectionTipIsOpen, value);
        }
        public string StorageFileUrl 
        { 
            get => _storageFileUrl;
            set => SetProperty(ref _storageFileUrl, value);
        }

        public string StorageFileContent
        {
            get => _storageFileContent;
            set => SetProperty(ref _storageFileContent, value);
        }
        public string FilePasswordHash 
        {
            get => _filePasswordHash;
            set => SetProperty(ref _filePasswordHash, value);
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
