using Prism.Commands;
using Prism.Navigation;
using Prism.Ioc;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.UWP.Utils;
using Project2FA.UWP.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Template10.Services.File;
using Template10.Services.Network;
using Windows.Storage;
using Windows.Storage.Pickers;
using Template10.Services.Secrets;
using Project2FA.UWP.Services.WebDAV;
using DecaTec.WebDav;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;
using Prism.Services.Dialogs;
using CommunityToolkit.Mvvm.Input;

namespace Project2FA.UWP.ViewModels
{
    public class UseDataFilePageViewModel : DatafileViewModelBase, IConfirmNavigationAsync
    {
        public ICommand UseDatafileCommand { get;}
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private IFileService FileService { get; }
        private INetworkService NetworkService { get; }
        private IDialogService DialogService { get; }
        private INavigationService NaviationService { get; }
        private ISecretService SecretService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(
            IFileService fileService, 
            INewtonsoftJSONService newtonsoftJSONService,
            INetworkService networkService,
            INavigationService navigationService,
            ISecretService secretService,
            IDialogService dialogService) : base(secretService,fileService, dialogService)
        {
            NaviationService = navigationService;
            DialogService = dialogService;
            NetworkService = networkService;
            NewtonsoftJSONService = newtonsoftJSONService;
            FileService = fileService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;
            SecretService =secretService;

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
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                await SetLocalFile();
                return true;
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
            if (await TestPassword())
            {
                await CreateLocalFileDB(isWebDAV);
                App.ShellPageInstance.NavigationIsAllowed = true;
                await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
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
            filePicker.FileTypeFilter.Add(".2fa");
            IsLoading = true;
            Windows.Storage.StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                IsLoading = false;
                LocalStorageFolder = await file.GetParentAsync();

                //set folder to the access list
                //StorageApplicationPermissions.FutureAccessList.Add(LocalStorageFolder, "metadata");
                StorageApplicationPermissions.FutureAccessList.Add(file, "metadata");

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

        private async Task<StorageFile> DownloadWebDAVFile(StorageFolder storageFolder)
        {
            StorageFile localFile;
            localFile = await storageFolder.CreateFileAsync(_choosenOneWebDAVFile.Name, CreationCollisionOption.ReplaceExisting);
            IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();

            WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
            using IRandomAccessStream randomAccessStream = await localFile.OpenAsync(FileAccessMode.ReadWrite);
            Stream targetStream = randomAccessStream.AsStreamForWrite();
            await client.Download(_choosenOneWebDAVFile.Path + "/" + _choosenOneWebDAVFile.Name, targetStream, progress, new System.Threading.CancellationToken());
            //TODO manipulate the file with the correct date time
            //var props = await localFile.Properties.GetDocumentPropertiesAsync();
            //await props.RetrievePropertiesAsync();
            return localFile;
        }

        /// <summary>
        /// Checks if the password is correct or not and displays an error message
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> TestPassword()
        {
            if (_choosenOneWebDAVFile != null)
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                return await TestPassword(await DownloadWebDAVFile(storageFolder), storageFolder);
            }
            else
            {
                StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
                return await TestPassword(file, LocalStorageFolder);
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

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            return !await DialogService.IsDialogRunning();
        }

        public bool ChangeDatafile { get; set; }
    }
}
