using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Prism.Regions;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.Uno.Core.File;
using WebDAVClient.Types;
using Windows.Storage;
using Windows.Storage.Pickers;
using DecaTec.WebDav;
using Windows.Storage.Streams;
using System.IO;
using Project2FA.Uno.Core.Network;
using Project2FA.Uno.Core.Secrets;
using Prism.Services.Dialogs;
using Prism.Ioc;
#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif


namespace Project2FA.ViewModels
{
    public class UseDataFilePageViewModel : DatafileViewModelBase
    {
        public ICommand UseDatafileCommand { get; }
        public ICommand SetAndCheckLocalDatafileCommand { get; }
        public ICommand SetAndCheckWebDAVDatafileCommand { get; }

        private IFileService FileService { get; }
        private INetworkService NetworkService { get; }
        private IDialogService DialogService { get; }
        private IRegionManager NaviationService { get; }

        private INewtonsoftJSONService NewtonsoftJSONService { get; }

        private bool _webDAVLoginRequiered;
        private bool _webDAVDatafilePropertiesExpanded;
        private bool _isWebDAVCreationButtonEnable;


        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UseDataFilePageViewModel(
            IContainerProvider containerProvider,
            IFileService fileService,
            INewtonsoftJSONService newtonsoftJSONService,
            INetworkService networkService,
            IRegionManager navigationService,
            IDialogService dialogService):base(containerProvider)
        {
            NaviationService = navigationService;
            DialogService = dialogService;
            NetworkService = networkService;
            NewtonsoftJSONService = newtonsoftJSONService;
            FileService = fileService;
            WebDAVLoginRequiered = true;
            WebDAVDatafilePropertiesEnabled = false;

            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
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
                await SetAndCheckLocalDatafile();
            });
#pragma warning restore AsyncFixer03 
            // Fire-and-forget async-void methods or delegates
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

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            WebDAVLoginCommand = new DelegateCommand(async () =>
            {
                IsLoading = true;
                (bool success, Status result) = await CheckServerStatus();
                if (success)
                {
                    if (ServerAddress != string.Empty && Username != string.Empty && WebDAVPassword != string.Empty)
                    {
                        if (await CheckLoginAsync())
                        {
                            WebDAVLoginRequiered = false;
                            WebDAVDatafilePropertiesEnabled = true;
                            WebDAVDatafilePropertiesExpanded = true;

                            IsLoading = false;
                            //var dialog = new WebViewDatafileContentDialog();
                            //var param = new DialogParameters();
                            //param.Add("CreateDatafileCase", false);
                            //param.Add("Status", result);
                            //ContentDialogResult dialogresult = await DialogService.ShowDialogAsync(dialog, param);
                            //if (dialogresult == ContentDialogResult.Primary)
                            //{
                            //    ChoosenOneWebDAVFile = dialog.ViewModel.ChoosenOneDatafile;
                            //}
                        }
                        else
                        {
                            //TODO error Message
                        }
                    }
                    else
                    {
                        //TODO error Message
                    }
                }
                else
                {
                    //TODO error Message
                }
                IsLoading = false;
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            SetAndCheckWebDAVDatafileCommand = new DelegateCommand(async () =>
            {
                await SetAndCheckLocalDatafile(true);
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
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

        public void ChooseWebDAV()
        {
            //ISecretService secretService = App.Current.Container.Resolve<ISecretService>();
            //if (secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") != string.Empty)
            //{
            //    ServerAddress = secretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress");
            //    WebDAVPassword = secretService.Helper.ReadSecret(Constants.ContainerName, "WDPassword");
            //    Username = secretService.Helper.ReadSecret(Constants.ContainerName, "WDUsername");
            //    //WebDAVLoginRequiered = false;
            //    //WebDAVDatafilePropertiesExpanded = true;
            //}
        }

        public async Task<bool> UseExistDatafile()
        {
            try
            {
                await SetLocalFile();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                //ErrorDialogs.UnauthorizedAccessUseLocalFileDialog();
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
                //await NaviationService.NavigateAsync("/" + nameof(AccountCodePage));
            }
            else
            {
                // TODO error Message
            }
            //IsLoading = false;
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
            //if (_choosenOneWebDAVFile != null)
            //{
            //    StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //    return await TestPassword(await DownloadWebDAVFile(storageFolder), storageFolder);
            //}
            //else
            //{
            //    StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
            //    return await TestPassword(file, LocalStorageFolder);
            //}
            StorageFile file = await LocalStorageFolder.GetFileAsync(DateFileName);
            return await TestPassword(file, LocalStorageFolder);
        }

        //private async Task<StorageFile> DownloadWebDAVFile(StorageFolder storageFolder)
        //{
        //    StorageFile localFile;
        //    localFile = await storageFolder.CreateFileAsync(_choosenOneWebDAVFile.Name, CreationCollisionOption.ReplaceExisting);
        //    IProgress<WebDavProgress> progress = new Progress<WebDavProgress>();

        //    WebDAVClient.Client client = WebDAVClientService.Instance.GetClient();
        //    using IRandomAccessStream randomAccessStream = await localFile.OpenAsync(FileAccessMode.ReadWrite);
        //    Stream targetStream = randomAccessStream.AsStreamForWrite();
        //    await client.Download(_choosenOneWebDAVFile.Path + "/" + _choosenOneWebDAVFile.Name, targetStream, progress, new System.Threading.CancellationToken());
        //    //TODO manipulate the file with the correct date time
        //    //var props = await localFile.Properties.GetDocumentPropertiesAsync();
        //    //await props.RetrievePropertiesAsync();
        //    return localFile;
        //}

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

        //public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        //{
        //    IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
        //    return !await dialogService.IsDialogRunning();
        //}

        public bool ChangeDatafile { get; set; }

        public bool WebDAVLoginRequiered
        {
            get => _webDAVLoginRequiered;
            set => SetProperty(ref _webDAVLoginRequiered, value);
        }

        public bool WebDAVDatafilePropertiesExpanded
        {
            get => _webDAVDatafilePropertiesExpanded;
            set => SetProperty(ref _webDAVDatafilePropertiesExpanded, value);
        }

        public WebDAVFileOrFolderModel ChoosenOneWebDAVFile
        {
            get => _choosenOneWebDAVFile;
            set => SetProperty(ref _choosenOneWebDAVFile, value);
        }

        public bool IsWebDAVCreationButtonEnable
        {
            get => _isWebDAVCreationButtonEnable;
            set => SetProperty(ref _isWebDAVCreationButtonEnable, value);
        }
    }
}
