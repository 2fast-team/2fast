using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Navigation;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Network;
using UNOversal.Services.Secrets;
using Project2FA.UNO.Views;
using System;
using System.Collections.Generic;
using System.Text;
using WinUI = Uno.UI;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Data;
using System.IO;
using UNOversal.Navigation;

namespace Project2FA.UNO.ViewModels
{
    [Bindable]
    public class UseDataFilePageViewModel : DatafileViewModelBase
    {
        public ICommand UseDatafileCommand { get; }
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private IFileService FileService { get; }
        //private INetworkService NetworkService { get; }
        private IDialogService DialogService { get; }
        private INavigationService NavigationService { get; }
        private ISecretService SecretService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(
            IFileService fileService,
            INewtonsoftJSONService newtonsoftJSONService,
            INetworkService networkService,
            ISecretService secretService,
            IDialogService dialogService)


        {
            Password = "blub";
            //base(secretService, fileService, dialogService)
            //NavigationService = navigationService;
            DialogService = dialogService;
            //NetworkService = networkService;
            NewtonsoftJSONService = newtonsoftJSONService;
            FileService = fileService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;
            SecretService = secretService;

            ConfirmErrorCommand = new RelayCommand(() =>
            {
                if (ShowError)
                {
                    ShowError = false;
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
            DatafileBTNActive = !string.IsNullOrEmpty(DateFileName) && !string.IsNullOrEmpty(Password);
            return Task.CompletedTask;
        }

        public async Task<bool> UseExistDatafile()
        {
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                //string path = @"C:\Windows\explorer.exe";
                //StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                await SetLocalFile();
                return true;
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
                App.ShellPageInstance.NavigationIsAllowed = true;
                await App.ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
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
            FileOpenPicker filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
#if __IOS__
            filePicker.FileTypeFilter.Add("com.jpwtechnology.2fa");
#else
            filePicker.FileTypeFilter.Add(".2fa");
#endif
            IsLoading = true;
            LocalStorageFile = await filePicker.PickSingleFileAsync();

            if (LocalStorageFile != null)
            {
                IsLoading = false;
                //set peristent access
#if __ANDROID__
                //Android.Net.Uri androidUri = Android.Net.Uri.Parse(LocalStorageFile.Path);
                //Android.App.Application.Context.ContentResolver.TakePersistableUriPermission(androidUri, Android.Content.ActivityFlags.GrantPersistableUriPermission);
#endif
                //LocalStorageFolder = await LocalStorageFile.GetParentAsync();

                //set folder to the access list
                //StorageApplicationPermissions.FutureAccessList.Add(LocalStorageFolder, "metadata");
                //StorageApplicationPermissions.FutureAccessList.Add(file, "metadata");

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


        /// <summary>
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {
            if (LocalStorageFolder != null)
            {
                StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
                return await TestPassword(file, LocalStorageFolder);
            }
            else
            {
                return await TestPassword(LocalStorageFile);
            }
            
            
        }

        private async Task<bool> TestPassword(StorageFile storageFile, StorageFolder storageFolder)
        {
            if (storageFile != null)
            {
                string datafileStr = await FileService.ReadStringAsync(storageFile.Name, storageFolder);
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
            }
            else
            {
                //TODO add error, no file found?
            }
            return false;
        }

        private async Task<bool> TestPassword(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                Stream datafileStream = null;
                string datafileStr;
#if __ANDROID__
                // create new thread for buggy Android, else NetworkOnMainThreadException 
                await Task.Run(async () => {
                    Android.Net.Uri androidUri = Android.Net.Uri.Parse(storageFile.Path);
                    datafileStream = Android.App.Application.Context.ContentResolver.OpenInputStream(androidUri);
                });
#elif __IOS__
       //         var opts = NSFileCoordinatorReadingOptions.WithoutChanges;
			    //var intents = NSFileAccessIntent.CreateReadingIntent(x, opts)).ToArray();
                //using var coordinator = new NSFileCoordinator();
#endif
                if (datafileStream != null)
                {
                    using (var reader = new StreamReader(datafileStream))
                    {
                        datafileStr = reader.ReadToEnd();
                    }
                    //string datafileStr = await FileIO.ReadTextAsync(storageFile);
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
                }
                else
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
