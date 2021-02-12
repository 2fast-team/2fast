using Prism.Commands;
using Prism.Navigation;
using Project2FA.Core.Services;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.Resources;
using Project2FA.Services.File;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Project2FA.ViewModels
{
    public class UseDatafilePageViewModel : ViewModelBase
    {

        private string _dateFileName;
        private bool _isPrimaryBTNEnable;
        private string _path;
        private string _password, _passwordRepeat;
        private bool _showError = false;
        private byte[] _iv;

        public ICommand PickLocalFileCommand { get; }
        public ICommand UseLocalFileCommand { get; }

        private INavigationService _navigationService { get; }
        private INewtonsoftJSONService _newtonsoftJSONService { get; }



        private IFileService _fileService { get; }  



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="navigationService"></param>
        public UseDatafilePageViewModel(INavigationService navigationService, 
            INewtonsoftJSONService newtonsoftJSONService, 
            IFileService fileService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _newtonsoftJSONService = newtonsoftJSONService;
            //_userDialogs = UserDialogs.Instance;
            _fileService = fileService;

            PickLocalFileCommand = new DelegateCommand(async () =>
            {
                await SetLocalFile();
            });
            UseLocalFileCommand = new DelegateCommand(UseLocalFile);
        }

        /// <summary>
        /// Checks the datafile and password inputs to enable / disable the continue button
        /// </summary>
        private void CheckInputs()
        {
            if (!string.IsNullOrEmpty(DateFileName))
            {
                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(PasswordRepeat))
                {
                    if (Password == PasswordRepeat)
                    {
                        IsPrimaryBTNEnable = true;
                    }
                    else
                    {
                        IsPrimaryBTNEnable = false;
                    }
                    //TODO dialog error: password!=password repeat
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

        /// <summary>
        /// Runs the file picker to pick the datafile
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetLocalFile()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "#use 2fa",
                    //FileTypes = PlatformTwoFactorAuthFileType()
                });
                if (file != null)
                {
                    Path = file.FullPath.Replace(file.FileName, string.Empty);
                    DateFileName = file.FileName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
                return false;
            }
        }
        // TODO iOS: DevicePlatform.iOS, new[] { 2fa...
        private static FilePickerFileType PlatformTwoFactorAuthFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/2fa"}  }, {DevicePlatform.UWP, new[] { "*.2fa" } }
            });

        private async void UseLocalFile()
        {
            //_userDialogs.ShowLoading(AppResource.Loading, MaskType.Black);
            if (TestPassword())
            {
                await CreateLocalFileDB();
                ////go back to the welcome page and then check if the database exists
                ////if so, the page should be navigate to the account page
                //await _navigationService.GoBackAsync();

                //_userDialogs.HideLoading();
                //use absolute uri path to reset the navigation stack!
                await _navigationService.NavigateAsync(new Uri("/NavigationPage/AccountCodePage", UriKind.Absolute));
            }
            else
            {
                //_userDialogs.HideLoading();
                if (ShowError)
                {
                    //_userDialogs.Toast(AppResource.WrongPassword);
                    ShowError = false;
                    //TODO error output, the password cannot decrypt the datafile
                }
                else
                {
                    //TODO something went wrong?
                }
            }
        }

        private async Task CreateLocalFileDB()
        {
            //local filedata

            var hash = CryptoService.CreateStringHash(Password);
            var passwordModel = await App.Repository.Password.UpsertAsync(new DBPasswordHashModel { Hash = hash });
            var datafile = await App.Repository.Datafile.UpsertAsync(
                new DBDatafileModel
                {
                    DBPasswordHashModel = passwordModel,
                    IsWebDAV = false,
                    Path = Path,
                    Name = DateFileName
                }); ;
            //write the password with the hash(key) in the secret vault
            await SecureStorage.SetAsync(hash, Password);
        }

        private bool TestPassword()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            var fileExist = _fileService.FileExists(DateFileName, StorageStrategies.Custom, Path);
            if (fileExist)
            {
                string datafileStr = _fileService.ReadString(DateFileName, StorageStrategies.Custom, Path);
                //read the iv for AES
                DatafileModel datafile = _newtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                _iv = datafile.IV;

                try
                {
                    var deserializeCollection = _newtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                        (Password, _iv, datafileStr);
                    return true;
                }
                catch (Exception)
                {
                    ShowError = true;
                    Password = string.Empty;

                    return false;
                }
            }
            return false;
        }

        public string DateFileName
        {
            get => _dateFileName;
            set
            {
                SetProperty(ref _dateFileName, value);
                CheckInputs();
            }
        }
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }
        public bool IsPrimaryBTNEnable
        {
            get => _isPrimaryBTNEnable;
            set => SetProperty(ref _isPrimaryBTNEnable, value);
        }
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                CheckInputs();
            }
        }

        public string PasswordRepeat
        {
            get => _passwordRepeat;
            set
            {
                SetProperty(ref _passwordRepeat, value);
                CheckInputs();
            }
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }
    }
}
