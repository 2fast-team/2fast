using Microsoft.Toolkit.Mvvm.Input;
using Prism.Commands;
using Project2FA.Repository.Models;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Template10.Services.File;
using Template10.Services.Serialization;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Project2FA.UWP.Strings;
using Windows.Storage;
using Template10.Services.Secrets;
using System.Windows.Input;
using Prism.Navigation;
using Project2FA.UWP.Views;

namespace Project2FA.UWP.ViewModels
{
    public class NewDataFilePageViewModel : DatafileViewModelBase
    {
        public ICommand CreateWebDAVDatafileCommand;
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
            ISecretService secretService,
            INavigationService navigationService,
            ISerializationService serializationService) : 
            base (secretService,fileService)
        {
            NaviationService = navigationService;
            SerializationService = serializationService;
            FileService = fileService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;

            SetAndCreateLocalDatafileCommand = new DelegateCommand(async () =>
            {
                await CreateLocalFile();
            });

            SetAndCreateWebDAVDatafileCommand = new DelegateCommand(async () =>
            {
                await CreateLocalFile(true);
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

            CreateWebDAVDatafileCommand = new DelegateCommand(() =>
            {

            });


            WebDAVLoginCommand = new DelegateCommand(async() =>
            {
                await WebDAVLogin(true);
            });
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

                            //ShowError = true;
                            //ErrorText = "#Die Passwörter stimmen nicht überein";

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

        private async Task CreateLocalFile (bool isWebDAV = false)
        {
            try
            {
                if (isWebDAV)
                {
                    LocalStorageFolder = ApplicationData.Current.LocalFolder;
                }
                Password = Password.Replace(" ", string.Empty);
                await CreateLocalFileDB(isWebDAV);
                byte[] iv = new AesManaged().IV;
                DatafileModel file = new DatafileModel() { IV = iv, Collection = new System.Collections.ObjectModel.ObservableCollection<TwoFACodeModel>() };
                await FileService.WriteStringAsync(DateFileName, SerializationService.Serialize(file), await StorageFolder.GetFolderFromPathAsync(LocalStorageFolder.Path));
                App.ShellPageInstance.NavigationIsAllowed = true;
                //uplaod is not necessary because the data file is empty.
                await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
            }
            catch (Exception exc)
            {
                if (exc is UnauthorizedAccessException)
                {
                    //TODO error message?
                }
                throw;
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
                //set folder to the access list
                StorageApplicationPermissions.FutureAccessList.Add(folder, "metadata");
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
