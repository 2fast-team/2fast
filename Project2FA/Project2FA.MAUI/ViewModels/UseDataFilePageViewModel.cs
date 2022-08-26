using CommunityToolkit.Mvvm.Input;
using DecaTec.WebDav;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Project2FA.Core.Services.JSON;
using Project2FA.Core.Services.WebDAV;
using Project2FA.MAUI.Helpers;
using Project2FA.MAUI.Services.JSON;
using Project2FA.MAUI.Views;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project2FA.MAUI.ViewModels
{
    public class UseDataFilePageViewModel : DatafileViewModelBase
    {
        public ICommand UseDatafileCommand { get; }
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private ISerializationService SerializationService { get; }
        private INewtonsoftJSONService NewtonsoftJSONService { get; }

        private FileResult _selectedFile;

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(ISerializationService serializationService, INewtonsoftJSONService newtonsoftJSONService)
        {
            SerializationService = serializationService;
            NewtonsoftJSONService = newtonsoftJSONService;

            ConfirmErrorCommand = new RelayCommand(() =>
            {
                if (ShowError)
                {
                    ShowError = false;
                }
            });

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            UseDatafileCommand = new RelayCommand(async () =>
            {
                await SetLocalFile(true); //change path is true
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            SetAndCheckLocalDatafileCommand = new RelayCommand(async () =>
            {
                await SetAndCheckLocalDatafile();
            });
#pragma warning restore AsyncFixer03 



#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            SetAndCheckWebDAVDatafileCommand = new RelayCommand(async () =>
            {
                await SetAndCheckLocalDatafile(true);
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            Password = "test";
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override Task CheckInputs()
        {
            DatafileBTNActive = !string.IsNullOrEmpty(DateFileName) && !string.IsNullOrEmpty(Password);
            return Task.CompletedTask;
        }

        public async Task<bool> UseExistDatafile()
        {
            try
            {
                PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (status == PermissionStatus.Granted)
                {
                    await SetLocalFile();
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (UnauthorizedAccessException)
            {
                //await ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                return false;
            }
        }

        public async Task SetAndCheckLocalDatafile(bool isWebDAV = false)
        {
            IsLoading = true;
            if (await TestPassword())
            {
                await CreateLocalFileDB(isWebDAV);
                //App.ShellPageInstance.NavigationIsAllowed = true;
                await Shell.Current.GoToAsync(nameof(AccountCodePage));
                //await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
            }
            else
            {
                // TODO error Message
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
            //	public.folder
            // "public.2fa-extension", "public.octet-stream", "com.jpwtechnology.2fa", "null", "application/octet-stream"

            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.jpwtechnology.2fa" } },
                    { DevicePlatform.Android, new[] { "application/json", "text/plain" , "application/octet-stream" } },
                    { DevicePlatform.WinUI, new[] { ".2fa" } },
                    { DevicePlatform.Tizen, new[] { "2fa" } },
                    { DevicePlatform.macOS, new[] { "2fa" } },
                });
            PickOptions options = new()
            {
                PickerTitle = "Please select a comic file",
                FileTypes = customFileType,
            };


            FileResult result = await FilePicker.Default.PickAsync(options);

            IsLoading = true;

            if (result != null)
            {
                IsLoading = false;

                //Environment.GetFolderPath(Environment.SpecialFolder.s);
                if (result.FileName.Contains(".2fa"))
                {
                    _selectedFile = result;
                    LocalStoragePath = result.FullPath;
                    DateFileName = result.FileName;
                    return true;
                }
                else
                {
                    return await SetLocalFile();
                }
                
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
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {
            //StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
            return await TestPassword(LocalStoragePath);
        }

        private async Task<bool> TestPassword(string storagePath)
        {

            if(_selectedFile != null)
            {
                //string datafileStr;
                //using (StreamReader reader = new StreamReader(await _selectedFile.OpenReadAsync()))
                //{
                //    datafileStr = reader.ReadToEnd();

                //}
                FileHelper fileHelper = new FileHelper();
                fileHelper.StartAccessFile(storagePath);
                string datafileStr = await File.ReadAllTextAsync(storagePath);
                //read the iv for AES
                DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                byte[] iv = datafile.IV;

                try
                {
                    DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                        (Password, iv, datafileStr);
                    return true;
                }
                catch (Exception)
                {
                    ShowError = true;
                    Password = string.Empty;

                    return false;
                }
                finally
                {
                    fileHelper.StopAccessFile(storagePath);
                }
            }
            else
            {
                //TODO add error, no file found?
            }
            return false;
        }

        //public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        //{
        //    IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
        //    return !await dialogService.IsDialogRunning();
        //}

        public bool ChangeDatafile { get; set; }
    }
}
