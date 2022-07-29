using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using Project2FA.Core.Services;
using Project2FA.MAUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project2FA.MAUI.ViewModels
{
    public partial class LoginPageViewModel : BindableBase
    {
        private bool _windowsHelloIsUsable, _isLogout;
        private string _password;

        public LoginPageViewModel()
        {
            Password = "test";
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
            var dbHash = await App.Repository.Password.GetAsync();
            string pwdhash = CryptoService.CreateStringHash(password);
            if (dbHash.Hash == pwdhash)
            {
                //App.Current.MainPage = App.AppShellInstance;
                await Shell.Current.GoToAsync("//" + nameof(AccountCodePage));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool WindowsHelloIsUsable
        {
            get => _windowsHelloIsUsable;
            set => SetProperty(ref _windowsHelloIsUsable, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public bool IsLogout
        {
            get => _isLogout;
            set => SetProperty(ref _isLogout, value);
        }
    }
}
