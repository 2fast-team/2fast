using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Project2FA.Repository.Models;
using Windows.Storage.Pickers;
using Project2FA.Core.Services.JSON;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Secrets;
using UNOversal.Services.File;
using System.Text;
#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    /// <summary>
    /// View model for the content dialog to use an existing datafile
    /// </summary>
    public class UseDatafileContentDialogViewModel : DatafileViewModelBase
    {
        public ICommand UseDatafileCommand { get; set; }
        private ISecretService SecretService { get; }

        private IFileService FileService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }

        private bool _changeDatafile;


        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDatafileContentDialogViewModel(
            ISecretService secretService,
            IFileService fileService,
            INewtonsoftJSONService newtonsoftJSONService
            ) :base()
        {
            SecretService = secretService;
            FileService = fileService;
            NewtonsoftJSONService = newtonsoftJSONService;
            ConfirmErrorCommand = new RelayCommand(() =>
            {
                ShowError = false;
            });
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            UseDatafileCommand = new RelayCommand(async () =>
            {
                await SetLocalFile(true); //change path is true
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override Task CheckInputs()
        {
            if (!string.IsNullOrEmpty(DateFileName))
            {
                if (!string.IsNullOrEmpty(Password))
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
                DatafileBTNActive = false;
            }
            return Task.CompletedTask;
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
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            filePicker.FileTypeFilter.Add(".2fa");
            IsLoading = true;
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                IsLoading = false;
                LocalStorageFolder = await file.GetParentAsync();

                //set folder to the access list
                //StorageApplicationPermissions.FutureAccessList.Add(LocalStorageFolder, "metadata");
                //StorageApplicationPermissions.FutureAccessList.Add(file, "metadata");

                DateFileName = file.Name;
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

        /// <summary>
        /// Creates a local DB with the data from the datafile
        /// </summary>
        public async Task CreateLocalFileDB()
        {
            // create local filedata
            if (SelectedIndex == 1)
            {
                IsLoading = true;
                await CreateDataFileSettings(false);
            }
        }

        /// <summary>
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {
            Windows.Storage.StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
            if (file != null)
            {
                string datafileStr = await FileService.ReadStringAsync(DateFileName, LocalStorageFolder);
                //read the iv for AES
                DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                var iv = datafile.IV;

                try
                {
                    var deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                        (Encoding.UTF8.GetBytes(Password), iv, datafileStr, datafile.Version);
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

        public bool ChangeDatafile { get => _changeDatafile; set => _changeDatafile = value; }
    }
}
