
using Prism.Commands;
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
using UNOversal.Services.Serialization;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;


#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class NewDataFilePageViewModel : DatafileViewModelBase
    {
        //public ICommand CreateWebDAVDatafileCommand;
        private IFileService FileService { get; }
        private ISerializationService SerializationService { get; }
        private INavigationService NaviationService { get; }
        public ICommand SetAndCreateLocalDatafileCommand { get; }
        public ICommand SetAndCreateWebDAVDatafileCommand { get; }
        private bool _selectWebDAV;

        private string _errorText;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewDataFilePageViewModel(
            IFileService fileService, 
            IDialogService dialogService,
            ISecretService secretService,
            INavigationService navigationService,
            ISerializationService serializationService) : 
            base ()
        {
            NaviationService = navigationService;
            SerializationService = serializationService;
            FileService = fileService;
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

            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });

            LoginCommand = new AsyncRelayCommand(CheckLoginAsync);

            ChangePathCommand = new AsyncRelayCommand(async () =>
            {
                await SetLocalPath(true); //change path is true
            });

            //CreateWebDAVDatafileCommand = new DelegateCommand(() =>
            //{

            //});


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
            await WebDAVLogin(true);
        }


        private async Task SetAndCreateLocalDatafile(bool isWebDAV)
        {
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }

            byte[] iv = Aes.Create().IV;
            DatafileModel file = new DatafileModel() { IV = iv, Collection = new System.Collections.ObjectModel.ObservableCollection<TwoFACodeModel>() };
            if (isWebDAV)
            {
                LocalStorageFolder = ApplicationData.Current.LocalFolder;
            }
            try
            {
                await FileService.WriteStringAsync(
                    DateFileName,
                    SerializationService.Serialize(file),
                    await StorageFolder.GetFolderFromPathAsync(LocalStorageFolder.Path));

                await CreateLocalFileDB(isWebDAV);
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
            }
            catch (Exception exc)
            {
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(SetAndCreateLocalDatafile),exc);
#endif
                var result = await Utils.ErrorDialogs.WritingDatafileError(true);
                if (result == ContentDialogResult.Primary)
                {
                    await SetAndCreateLocalDatafile(isWebDAV).ConfigureAwait(false);
                }
            }

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
    }
}
