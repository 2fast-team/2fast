using Prism.Commands;
using System;
using Windows.Storage;
using Prism.Ioc;
using Template10.Services.File;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Project2FA.UWP.Strings;
using System.Security.Cryptography;
using Project2FA.Repository.Models;
using Template10.Services.Serialization;

namespace Project2FA.UWP.ViewModels
{
    public class NewDatafileContentDialogViewModel : DatafileViewModelBase
    {
        private IFileService FileService { get; }
        private ISerializationService SerializationService { get; }

        private string _errorText;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewDatafileContentDialogViewModel()
        {
            SerializationService = App.Current.Container.Resolve<ISerializationService>();
            FileService = App.Current.Container.Resolve<IFileService>();

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                Password = Password.Replace(" ", string.Empty);
                await CreateLocalFileDB(false);
                byte[] iv = new AesManaged().IV;
                DatafileModel file = new DatafileModel() { IV = iv, Collection = new System.Collections.ObjectModel.ObservableCollection<TwoFACodeModel>() };
                await FileService.WriteStringAsync(DateFileName, SerializationService.Serialize(file), await StorageFolder.GetFolderFromPathAsync(LocalStorageFolder.Path));
                App.ShellPageInstance.NavigationIsAllowed = true;
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            CheckServerAddressCommand = new DelegateCommand(() =>
            {
                CheckServerStatus();
            });
            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });

            LoginCommand = new DelegateCommand(async () =>
            {
                CheckLoginAsync();
            });

            ChangePathCommand = new DelegateCommand( async() =>
            {
                SetLocalPath(true); //change path is true
            });


            ChooseWebDAVCommand = new DelegateCommand(() =>
            {

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
    }
}
