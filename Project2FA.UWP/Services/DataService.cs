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
using Template10.Services.Dialog;
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
        public AdvancedCollectionView ACVCollection { get; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();
        private bool _emptyAccountCollectionTipIsOpen;

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
            ACVCollection.SortDescriptions.Add(new SortDescription("Label", SortDirection.Ascending));
            Collection.CollectionChanged += Accounts_CollectionChanged;
            CheckTime();
            CheckLocalDatafile();
        }

        /// <summary>
        /// Check if the current time of the system is correct
        /// </summary>
        private async Task CheckTime()
        {
            TimeSpan difference = DateTime.UtcNow - SettingsService.Instance.LastCheckedSystemTime;
            // check the time again after 8 hours or always in debug mode
            if (System.Diagnostics.Debugger.IsAttached || difference.TotalHours > 8)
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
                            await SystemTimeNotCorrectError();
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
                            // initialize the newest (last) item
                            InitializeItem((sender as ObservableCollection<TwoFACodeModel>).Count -1);
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
            await CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < Collection.Count; i++)
            {
                InitializeItem(i);
            }
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Reset the period and regnerate the TOTP code
        /// </summary>
        /// <param name="i">The index of the item in the collection</param>
        private void InitializeItem(int i)
        {
            Collection[i].Seconds = Collection[i].Period;
            GenerateTOTP(i);
        }

        /// <summary>
        /// Reloads the datafile in the local database
        /// </summary>
        public async Task ReloadDatafile()
        {
            await CollectionAccessSemaphore.WaitAsync();
            await CheckLocalDatafile();
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Checks and reads the current local datafile
        /// </summary>
        /// <param name="dbDatafile"></param>
        private async Task CheckLocalDatafile()
        {
            try
            {
                DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
                if (await FileService.FileExistsAsync(dbDatafile.Name, folder))
                {
                    DBPasswordHashModel dbHash = await App.Repository.Password.GetAsync();
                    // prevent write of the datafile
                    _initialization = true;
                    try
                    {
                        string datafileStr = await FileService.ReadStringAsync(dbDatafile.Name, folder);
                        if (!string.IsNullOrEmpty(datafileStr))
                        {
                            // read the iv for AES
                            DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            byte[] iv = datafile.IV;

                            datafile = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                                            SecretService.Helper.ReadSecret(Constants.ContainerName,dbHash.Hash),
                                                            iv,
                                                            datafileStr);
                            deserializeCollection = datafile.Collection;
                        }

                    }
                    catch (Exception)
                    {
                        ShowPasswordError();
                    }
                }
                // file not found case
                else
                {
                    ShowFileOrFolderNotFoundError();
                }

                if (deserializeCollection != null)
                {
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
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (exc is UnauthorizedAccessException)
                {
                    ShowUnauthorizedAccessError();
                }
                else if(exc is FileNotFoundException)
                {
                    ShowFileOrFolderNotFoundError();
                }
                else
                {
                    _errorOccurred = true;
                    TrackingManager.TrackException(exc);
                    ErrorDialogs.ShowUnexpectedError(exc);
                }

            }
            // writing the data file is activated again
            _initialization = false;
        }

        /// <summary>
        /// Deletes the datafile from storage
        /// </summary>
        public async Task DeleteLocalDatafile()
        {
            DBDatafileModel dbDatafile = await App.Repository.Datafile.GetAsync();
            StorageFile storageFile = await (await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path)).GetFileAsync(dbDatafile.Name);

            if (storageFile != null)
            {
                await storageFile.DeleteAsync();
            }
        }

        private void ErrorResolved()
        {
            _errorOccurred = false;
            // allow shell navigation
            App.ShellPageInstance.NavigationIsAllowed = true;
        }

        private async Task ShowUnauthorizedAccessError()
        {
            _errorOccurred = true;
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AuthorizationFileSystemContentDialogTitle;
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            dialog.Content = markdown;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                Prism.PrismApplicationBase.Current.Exit();
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonText = Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });
            ContentDialogResult result = await DialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                Prism.PrismApplicationBase.Current.Exit();
            }
        }

        /// <summary>
        /// Displays a wrong password error message and option to change the password
        /// </summary>
        private async Task ShowPasswordError()
        {
            _errorOccurred = true;
            ContentDialog dialog = new ContentDialog
            {
                Title = Resources.PasswordInvalidHeader,
                Content = Resources.PasswordInvalidMessage,
                PrimaryButtonText = Resources.ChangePassword,
                PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style,

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                PrimaryButtonCommand = new DelegateCommand(async () =>
                {
                    ContentDialogResult result = await DialogService.ShowAsync(new ChangeDatafilePasswordContentDialog(true));
                }),
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

                SecondaryButtonText = Resources.CloseApp,
                SecondaryButtonCommand = new DelegateCommand(() =>
                {
                    Prism.PrismApplicationBase.Current.Exit();
                })
            };

            ContentDialogResult result = await DialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                ShowPasswordError();
            }
        }

        /// <summary>
        /// Displays a FileNotFoundException message and the option for factory reset or correcting the path
        /// </summary>
        private async Task ShowFileOrFolderNotFoundError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            }
            catch (UnauthorizedAccessException)
            {
                ErrorDialogs.UnauthorizedAccessDialog();
            }
            _errorOccurred = true;
            // disable shell navigation
            App.ShellPageInstance.NavigationIsAllowed = false;
            Logger.Log("no datafile found", Category.Exception, Priority.High);
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
                ContentDialogResult result = await dialogService.ShowAsync(new UpdateDatafileContentDialog());
                if (result == ContentDialogResult.Primary)
                {
                    ErrorResolved();
                    await CheckLocalDatafile();
                }
                if (result == ContentDialogResult.None)
                {
                    ShowFileOrFolderNotFoundError();
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
                        await dialogService.ShowAsync(dialog);
                    }
                }
            }

            await dialogService.ShowAsync(dialog);
        }

        /// <summary>
        /// Displays that the system time is not correct
        /// </summary>
        /// <returns></returns>
        private Task SystemTimeNotCorrectError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AccountCodePageWrongTimeTitle;
            dialog.Content = Resources.AccountCodePageWrongTimeContent;
            dialog.PrimaryButtonText = Resources.AccountCodePageWrongTimeBTN;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:dateandtime"));
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.SecondaryButtonText = Resources.Confirm;
            return dialogService.ShowAsync(dialog);
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

            DatafileModel file = new DatafileModel() { IV = iv, Collection = Collection };

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(datafileDB.Path);
            await FileService.WriteStringAsync(
                    datafileDB.Name,
                    NewtonsoftJSONService.SerializeEncrypt(SecretService.Helper.ReadSecret(Constants.ContainerName, dbHash.Hash),
                    iv, 
                    file),
                    folder);
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Generates a TOTP code for the i'th entry of a collection
        /// </summary>
        /// <param name="i"></param>
        public async Task<bool> GenerateTOTP(int i)
        {
            try
            {
                if (Collection[i].SecretByteArray is null)
                {
                    TrackingManager.TrackEvent(Category.Warn, Priority.High, "Secret key is empty!");
                    await ReloadDatafile();
                    return false;
                }
                else
                {
                    Totp totp = new Totp(Collection[i].SecretByteArray, Collection[i].Period, Collection[i].HashMode, Collection[i].TotpSize);
                    int remainingTime = totp.RemainingSeconds();
                    bool verified = totp.VerifyTotp(totp.ComputeTotp(), out long matched, VerificationWindow.RfcSpecifiedNetworkDelay);
                    if (!verified)
                    {
                        return false;
                    }
                    if (_checkedTimeSynchronisation && _ntpServerTimeDifference != null)
                    {
                        Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow.AddMilliseconds(_ntpServerTimeDifference.TotalMilliseconds));
                    }
                    else
                    {
                        Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.High);
                TrackingManager.TrackException(ex);
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
