using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project2FA.Core.Services;
using Project2FA.Core.Services.JSON;
using Project2FA.MAUI.Services;
using Project2FA.MAUI.Views;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.ViewModels
{
    public partial class FileActivationPageViewModel : ObservableObject
    {
        private string _password;
        private INewtonsoftJSONService NewtonsoftJSONService { get; }
        private string _error;

        public FileActivationPageViewModel(INewtonsoftJSONService newtonsoftJSONService)
        {
            Password = "test";
            NewtonsoftJSONService = newtonsoftJSONService;
        }

        /// <summary>
        /// Checks the password for the login
        /// </summary>
        [RelayCommand]
        private async void CheckLogin()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                if (!await CheckNavigationRequest(Password))
                {
                    Password = string.Empty;
                    //await ShowLoginError();
                }
            }
        }

        /// <summary>
        /// Check if the input have the same hash as the saved password
        /// and set the Windows content to the ShellPage if true
        /// </summary>
        /// <param name="password"></param>
        /// <returns>return true if password hash is valid;
        /// else when the hash is not equal to the saved password</returns>
        private async Task<bool> CheckNavigationRequest(string password)
        {
            if (await TestPassword(DataService.Instance.StorageFileUrl))
            {
                string hash = CryptoService.CreateStringHash(Password, false);
                DataService.Instance.FilePasswordHash = hash;
                await SecureStorage.Default.SetAsync(hash, Password);
                await Shell.Current.GoToAsync("//" + nameof(AccountCodePage));
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> TestPassword(string storagePath)
        {
            try
            {
                string datafileStr = !string.IsNullOrWhiteSpace(DataService.Instance.StorageFileUrl) ? await File.ReadAllTextAsync(storagePath) : DataService.Instance.StorageFileContent;
                //read the iv for AES
                DatafileModel datafile = NewtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                byte[] iv = datafile.IV;
                DatafileModel deserializeCollection = NewtonsoftJSONService.DeserializeDecrypt<DatafileModel>
                    (Password, iv, datafileStr);
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
                ShowError = true;
                Password = string.Empty;

                return false;
            }
        }

        public bool ShowError { get; private set; }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string Path 
        {
            get => DataService.Instance.StorageFileUrl;
        }
        public string Error 
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }
    }
}
