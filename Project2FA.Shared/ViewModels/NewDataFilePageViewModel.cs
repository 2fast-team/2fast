
using Project2FA.Repository.Models;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Project2FA.Strings;
using Windows.Storage;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.File;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using Project2FA.Core;
using UNOversal.Services.Logging;
using Project2FA.Services;
using UNOversal.Services.Serialization;


#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml.Data;
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public partial class NewDataFilePageViewModel : DatafileViewModelBase
    {
        private IFileService FileService { get; }

        private IDialogService DialogService { get; }
        private ISerializationService SerializationService { get; }
        private INavigationService NaviationService { get; }
        public ICommand SetAndCreateLocalDatafileCommand { get; }
        public ICommand SetAndCreateWebDAVDatafileCommand { get; }
        private ILoggingService LoggingService { get; }
        private bool _selectWebDAV;

        private string _errorText, _folderPath;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewDataFilePageViewModel(
            IFileService fileService, 
            IDialogService dialogService,
            INavigationService navigationService,
            ISerializationService serializationService,
            ILoggingService loggingService) : 
            base()
        {
            NaviationService = navigationService;
            SerializationService = serializationService;
            DialogService = dialogService;
            FileService = fileService;
            LoggingService = loggingService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;

            SetAndCreateLocalDatafileCommand = new AsyncRelayCommand(async () =>
            {
                await SetAndCreateLocalDatafile(false);
            });

            SetAndCreateWebDAVDatafileCommand = new AsyncRelayCommand(async () =>
            {
                await SetAndCreateLocalDatafile(true);
            });


            CheckServerAddressCommand = new AsyncRelayCommand(CheckServerStatus);

            ConfirmErrorCommand = new RelayCommand(() =>
            {
                ShowError = false;
            });

            LoginCommand = new AsyncRelayCommand(CheckLoginAsync);

            ChangePathCommand = new AsyncRelayCommand(async () =>
            {
#if !WINDOWS_UWP
                await SaveFileAsync();
#else
                await SetLocalPath(true); //change path is true
#endif
            });

            WebDAVLoginCommand = new AsyncRelayCommand(WebDAVLoginCommandTask);
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override async Task CheckInputs()
        {
            if (!string.IsNullOrEmpty(DateFileName))
            {
                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(PasswordRepeat))
                {
                    if (SelectWebDAV)
                    {
                        if (Password == PasswordRepeat)
                        {
                            DatafileBTNActive = true;
                        }
                        else
                        {
                            DatafileBTNActive = false;
                        }
                    }
                    else
                    {
                        if (!await CheckIfNameExists(DateFileName + ".2fa"))
                        {
                            if (Password == PasswordRepeat && (LocalStorageFolder != null || LocalStorageFile != null))
                            {
                                DatafileBTNActive = true;
                            }
                            else
                            {
                                DatafileBTNActive = false;
                            }
                        }
                        else
                        {
                            ShowError = true;
                            ErrorText = Resources.NewDatafileContentDialogDatafileExistsError;
                        }
                    }
                }
                else
                {
                    DatafileBTNActive = false;
                }
            }
            else
            {
                DatafileBTNActive = false;
            }
        }

        private async Task WebDAVLoginCommandTask()
        {
            // TODO WebDAV authentication via WebView2
//#if WINDOWS_UWP
//            var dialog = new WebDAVAuthContentDialog();
//            var dialogparams = new DialogParameters();
//            dialogparams.Add("url", "https://SERVER/index.php/login/flow");
//            await DialogService.ShowDialogAsync(dialog, dialogparams);

//#endif
            await WebDAVLogin(true);
        }


        private async Task SetAndCreateLocalDatafile(bool isWebDAV)
        {
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }

            DatafileModel model = new DatafileModel() { IV = Aes.Create().IV, Collection = new System.Collections.ObjectModel.ObservableCollection<TwoFACodeModel>(), Version = 2 };
            if (isWebDAV)
            {
                LocalStorageFolder = ApplicationData.Current.LocalFolder;
            }
            try
            {
                bool created;
                if (LocalStorageFolder != null)
                {
                    created = await FileService.WriteStringAsync(
                        DateFileName,
                        SerializationService.Serialize(model),
                        LocalStorageFolder);
                }
                else
                {
#if __ANDROID__
                    // create new thread for buggy Android, else NetworkOnMainThreadException 
                    await Task.Run(async () => {
                        await FileIO.WriteTextAsync(LocalStorageFile, SerializationService.Serialize(model));
                    });
#else
                    await FileIO.WriteTextAsync(LocalStorageFile, SerializationService.Serialize(model));
#endif
                }

#if !WINDOWS_UWP
                // load file from local folder
                //LocalStorageFile = await LocalStorageFolder.GetFileAsync(DateFileName);
#endif

                var result = await CreateDataFileSettings(isWebDAV);
                if (result)
                {
                    App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                    await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
                else
                {
                    // TODO error Dialog
                }

            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(SetAndCreateLocalDatafile),exc);
#endif
                if (exc.Message.Contains("E_ACCESSDENIED"))
                {
                    var dialog = new ContentDialog();
                    dialog.Title = Resources.NewDatafileAccessErrorTitle;
                    dialog.Content = Resources.NewDatafileAccessErrorDesc;
                    dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
                    dialog.PrimaryButtonText = Resources.Confirm;
                    await DialogService.ShowDialogAsync(dialog, new DialogParameters());
                }
                else
                {
                    var result = await Utils.ErrorDialogs.WritingDatafileError(true);
                    if (result == ContentDialogResult.Primary)
                    {
                        await SetAndCreateLocalDatafile(isWebDAV);
                    }
                }
            }
        }

        private async Task<bool> SaveFileAsync()
        {
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }

            var saveFilePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = DateFileName
            };
#if ANDROID
            var androidFlags = Android.Content.ActivityFlags.GrantReadUriPermission |
                Android.Content.ActivityFlags.GrantWriteUriPermission |
                Android.Content.ActivityFlags.GrantPersistableUriPermission |
                Android.Content.ActivityFlags.GrantPrefixUriPermission;
            // set the persistent access to the file
            FilePickerHelper.RegisterOnBeforeStartActivity(saveFilePicker, (intent) =>
            {
                intent.AddFlags(androidFlags);
            });
#endif
            IsLoading = true;
            var file = await saveFilePicker.PickSaveFileAsync();
            IsLoading = false;
            if (file != null)
            {
                FolderPath = file.Path.Replace(file.Name, string.Empty);
                LocalStorageFile = file;

                CheckInputs();
                return true;
            }
            else
            {
                //the path, but cancel the dialog 
                return false;
            }
            //saveFilePicker.FileTypeChoices.Add("2FA files", new System.Collections.Generic.List<string>() { ".2fa" });
        }


        /// <summary>
        /// Starts a folder picker to pick a datafile from a local path
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> SetLocalPath(bool changePath = false)
        {
            SelectedIndex = 1;
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            IsLoading = true;
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                IsLoading = false;
#if WINDOWS_UWP
                //set folder to the access list
                StorageApplicationPermissions.FutureAccessList.Add(folder, "metadata");
#endif
                LocalStorageFolder = folder;

                CheckInputs();
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
#region GetSet

        public string ErrorText
        {
            get => _errorText;
            set
            {
                SetProperty(ref _errorText, value);
            }
        }

        public bool SelectWebDAV 
        { 
            get => _selectWebDAV;
            set => SetProperty(ref _selectWebDAV, value);
        }
        public string FolderPath 
        { 
            get => _folderPath;
            set => SetProperty(ref _folderPath, value);
        }
        #endregion
    }
}
