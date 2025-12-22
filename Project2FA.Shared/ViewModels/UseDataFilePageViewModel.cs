using Project2FA.Repository.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using DecaTec.WebDav;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Navigation;
using UNOversal.Services.File;
using UNOversal.Services.Network;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using Project2FA.Utils;
using Project2FA.Services.WebDAV;
using System.Text;
using UNOversal.Services.Logging;
using Project2FA.Services;
using UNOversal.Services.Serialization;
using Project2FA.Core.Services.Crypto;



#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.Security.Authorization.AppCapabilityAccess;
#else
#if __ANDROID__ || __IOS__
using static Uno.WinRTFeatureConfiguration;
#endif
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
using Uno.UI;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public partial class UseDataFilePageViewModel : DatafileViewModelBase, IConfirmNavigationAsync
    {
        public ICommand UseDatafileCommand { get;}
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private IFileService FileService { get; }
        private INetworkService NetworkService { get; }
        private IDialogService DialogService { get; }
        private INavigationService NaviationService { get; }
        private ISecretService SecretService { get; }
        private ILoggingService LoggingService { get; }

        private ISerializationService SerializationService { get; }
        private ISerializationCryptoService SerializationCryptoService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(
            IFileService fileService,
            ISerializationService serializationService,
            ISerializationCryptoService serializationCryptoService,
            INetworkService networkService,
            INavigationService navigationService,
            ISecretService secretService,
            IDialogService dialogService,
            ILoggingService loggingService) : base()
        {
            NaviationService = navigationService;
            DialogService = dialogService;
            NetworkService = networkService;
            SerializationService = serializationService;
            SerializationCryptoService = serializationCryptoService;
            FileService = fileService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;
            SecretService =secretService;
            LoggingService = loggingService;

            ConfirmErrorCommand = new RelayCommand(() =>
            {
                if (ShowError)
                {
                    ShowError = false;
                }

                if (WebDAVLoginError)
                {
                    WebDAVLoginError = false;
                }
            });


            UseDatafileCommand = new AsyncRelayCommand(async () =>
            {
                await SetLocalFile(true); //change path is true
            });

            SetAndCheckLocalDatafileCommand = new AsyncRelayCommand(async () =>
            {
                await SetAndCheckLocalDatafile();
            });


            WebDAVLoginCommand = new AsyncRelayCommand(async () =>
            {
                await WebDAVLogin(false);
            });


            SetAndCheckWebDAVDatafileCommand = new AsyncRelayCommand(async () =>
            {
                await SetAndCheckLocalDatafile(true);
            });
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override Task CheckInputs()
        {
            if (ChoosenOneWebDAVFile != null)
            {
                IsWebDAVCreationButtonEnable = !string.IsNullOrWhiteSpace(Password);
            }
            else
            {
                DatafileBTNActive = !string.IsNullOrEmpty(DateFileName) && !string.IsNullOrEmpty(Password);
            }
            return Task.CompletedTask;
        }

        public async Task<bool> UseExistDatafile()
        {
            try
            {
#if WINDOWS_UWP
                // check if the app has file system access to load the file
                var checkFileSystemAccess = AppCapability.Create("broadFileSystemAccess").CheckAccess();
                switch (checkFileSystemAccess)
                {
                    case AppCapabilityAccessStatus.DeniedBySystem:
                    case AppCapabilityAccessStatus.NotDeclaredByApp:
                    case AppCapabilityAccessStatus.DeniedByUser:
                    case AppCapabilityAccessStatus.UserPromptRequired:
                    default:
                        await ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                        break;
                    case AppCapabilityAccessStatus.Allowed:
                        return await SetLocalFile();
                }
                return await UseExistDatafile();
#else
                return await SetLocalFile();
#endif

            }
            catch (UnauthorizedAccessException)
            {
                await ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                return false;
            }
        }

        public async Task SetAndCheckLocalDatafile(bool isWebDAV = false)
        {
            IsLoading = true;

#if WINDOWS_UWP
            if (await TestPassword() && (LocalStorageFolder != null || isWebDAV))
#else
            if (await TestPassword())
#endif
            {
                if (await CreateDataFileSettings(isWebDAV))
                {
                    App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                    await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
                else
                {
                    // TODO error dialog
                }

            }
            IsLoading = false;
        }

        /// <summary>
        /// Opens a file picker to choose a local .2fa datafile
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetLocalFile(bool changePath = false)
        {
            if (!changePath)
            {
                SelectedIndex = 1;
            }
            FileOpenPicker filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            filePicker.FileTypeFilter.Add(".2fa");
#if __IOS__
            // mapping the filter type to definied UTType
            
            FileTypes.FileTypeToUTTypeMapping.Add(".2fa", "com.jpwtechnology.2fa");
#endif

#if ANDROID
            var androidFlags = Android.Content.ActivityFlags.GrantReadUriPermission |
                Android.Content.ActivityFlags.GrantWriteUriPermission |
                Android.Content.ActivityFlags.GrantPersistableUriPermission |
                Android.Content.ActivityFlags.GrantPrefixUriPermission;
            // set the persistent access request to the picker
            FilePickerHelper.RegisterOnBeforeStartActivity(filePicker, (intent) =>
            {
                intent.AddFlags(androidFlags);
            });
#endif

            IsLoading = true;
            LocalStorageFile = await filePicker.PickSingleFileAsync();
            if (LocalStorageFile != null)
            {
                IsLoading = false;
#if WINDOWS_UWP
                // set folder to the access list
                LocalStorageFolder = await LocalStorageFile.GetParentAsync();
                StorageApplicationPermissions.FutureAccessList.Add(LocalStorageFile, "metadata");
#endif

                DateFileName = LocalStorageFile.Name;
                return true;
            }
            else
            {
                //prevents the change of the index, if the user want to change
                //the path, but cancel the dialog 
                IsLoading = false;
                if (!changePath)
                {
                    SelectedIndex = 0;
                }
                return false;
            }
        }

        private async Task<StorageFile> DownloadWebDAVFile(StorageFolder storageFolder)
        {
            try
            {
                StorageFile localFile;
                localFile = await storageFolder.CreateFileAsync(ChoosenOneWebDAVFile.Name, CreationCollisionOption.ReplaceExisting);
                IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();

                WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
                using IRandomAccessStream randomAccessStream = await localFile.OpenAsync(FileAccessMode.ReadWrite);
                Stream targetStream = randomAccessStream.AsStreamForWrite();
                await client.Download(ChoosenOneWebDAVFile.Path + "/" + ChoosenOneWebDAVFile.Name, targetStream, progress, new System.Threading.CancellationToken());
                //TODO manipulate the file with the correct date time
                //var props = await localFile.Properties.GetDocumentPropertiesAsync();
                //await props.RetrievePropertiesAsync();
                DateFileName = localFile.Name;
                return localFile;
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP
                TrackingManager.TrackException(nameof(DownloadWebDAVFile),exc);
#endif
                return null;
            }

        }

        /// <summary>
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {
            if (ChoosenOneWebDAVFile != null)
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                LocalStorageFile = await DownloadWebDAVFile(storageFolder);
                return await TestPasswordInternal();
            }
            else
            {
                return await TestPasswordInternal();
            }
        }

        private async Task<bool> TestPasswordInternal()
        {
            if (LocalStorageFile != null)
            {
                string datafileStr = string.Empty;
                // create new thread for buggy Android, else NetworkOnMainThreadException 
#if __ANDROID__
                await Task.Run(async () => {

                    datafileStr = await FileIO.ReadTextAsync(LocalStorageFile);

                });
#else
                datafileStr = await FileIO.ReadTextAsync(LocalStorageFile);
#endif
                //read the iv for AES
                DatafileModel datafile = SerializationService.Deserialize<DatafileModel>(datafileStr);

                try
                {
                    DatafileModel deserializeCollection = SerializationCryptoService.DeserializeDecrypt<DatafileModel>
                        (CryptoService.CreateByteArrayKeyV2(Encoding.UTF8.GetBytes(Password)), datafile.IV, datafileStr, datafile.Version);
                    return true;
                }
                catch (Exception)
                {
                    ShowError = true;
                    Password = string.Empty;

                    return false;
                }
            }
            else
            {
                //TODO add error, no file found?
            }
            return false;
        }

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            return !await DialogService.IsDialogRunning();
        }

        public bool ChangeDatafile { get; set; }
    }
}
