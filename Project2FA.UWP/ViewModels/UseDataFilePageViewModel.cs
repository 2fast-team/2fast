using Prism.Commands;
using Prism.Navigation;
using Prism.Ioc;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.UWP.Utils;
using Project2FA.UWP.Views;
using Project2FA.UWP.Views.ContentDialogs;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Windows.Input;
using Template10.Services.Dialog;
using Template10.Services.File;
using Template10.Services.Network;
using WebDAVClient.Types;
using Windows.Storage;
using Windows.Storage.Pickers;
using Template10.Services.Secrets;
using Project2FA.Core;

namespace Project2FA.UWP.ViewModels
{
    public class UseDataFilePageViewModel : DatafileViewModelBase, IConfirmNavigationAsync
    {
        public ICommand UseDatafileCommand { get;}
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private IFileService FileService { get; }

        private IResourceService ResourceService { get; }
        private INetworkService NetworkService { get; }
        private IDialogService DialogService { get; }
        private INavigationService NaviationService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }


        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(
            IFileService fileService, 
            INewtonsoftJSONService newtonsoftJSONService,
            INetworkService networkService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            NaviationService = navigationService;
            DialogService = dialogService;
            NetworkService = networkService;
            NewtonsoftJSONService = newtonsoftJSONService;
            FileService = fileService;

            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });

            WebDAVLoginCommand = new DelegateCommand(() =>
            {

            });

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            UseDatafileCommand = new DelegateCommand(async () =>
            {
                await SetLocalFile(true); //change path is true
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            SetAndCheckLocalDatafileCommand = new DelegateCommand(async () =>
            {
                if (await TestPassword())
                {
                    await CreateLocalFileDB(false);
                    await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
                else
                {
                    //error Message
                }
            });

//#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

//            ChooseWebDAVCommand = new DelegateCommand(() =>
//            {
//                SelectedIndex = 1;
//                var secretService = App.Current.Container.Resolve<ISecretService>();
//                if (secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") != string.Empty)
//                {
//                    ServerAddress = secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress");
//                    Password = secretService.Helper.ReadSecret(Constants.ContainerName, "WDPassword");
//                    Username = secretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername");
//                }
//            });

//#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates

            WebDAVLoginCommand = new DelegateCommand(async () =>
            {
                (bool success, Status result) = await CheckServerStatus();
                if (success)
                {
                    if (ServerAddress != string.Empty && Username != string.Empty && Password != string.Empty)
                    {
                        if (await CheckLoginAsync())
                        {
                            await DialogService.ShowAsync(new WebViewDatafileContentDialog(false, result));
                        }
                    }
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            SetAndCheckWebDAVDatafileCommand = new DelegateCommand(async () =>
            {
                if (await TestPassword())
                {
                    await CreateLocalFileDB(true);
                    await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
                }
                else
                {
                    //error Message
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override void CheckInputs()
        {
            if (!string.IsNullOrEmpty(DateFileName))
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    IsPrimaryBTNEnable = true;
                }
                else
                {
                    IsPrimaryBTNEnable = false;
                }
            }
            else
            {
                IsPrimaryBTNEnable = false;
            }
        }

        public void ChooseWebDAV()
        {
            var secretService = App.Current.Container.Resolve<ISecretService>();
            if (secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") != string.Empty)
            {
                ServerAddress = secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress");
                WebDAVPassword = secretService.Helper.ReadSecret(Constants.ContainerName, "WDPassword");
                Username = secretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername");
            }
        }

        public async Task<bool> UseExistDatafile()
        {
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                await SetLocalFile();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
                return false;
            }
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
            IsLoading = true;
            Windows.Storage.StorageFile file = await filePicker.PickSingleFileAsync();
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

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            return !await dialogService.IsDialogRunning();
        }

        public bool ChangeDatafile { get; set; }
    }
}
