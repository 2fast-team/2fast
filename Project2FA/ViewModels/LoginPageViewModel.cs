using Prism.Commands;
using Prism.Navigation;
using Project2FA.Core.Services;
using Project2FA.Resources;
using System;
using System.Windows.Input;

namespace Project2FA.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private string _password;
        public ICommand LoginCommand { get; }
        INavigationService _navigationService { get; }

        public LoginPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoginCommand = new DelegateCommand(CheckLogin);
        }

        private async void CheckLogin()
        {
            //_userDialogs.ShowLoading(AppResource.Loading, MaskType.Black);
            if (!string.IsNullOrEmpty(Password))
            {
                var dbHash = await App.Repository.Password.GetAsync();
                //_userDialogs.HideLoading();
                if (dbHash.Hash == CryptoService.CreateStringHash(Password))
                {
                    //use absolute uri path to reset the navigation stack!
                    await _navigationService.NavigateAsync(new Uri("/NavigationPage/AccountCodePage", UriKind.Absolute));
                }
                else
                {
                    //_userDialogs.Toast(AppResource.WrongPassword);
                }
            }
            else
            {
                //_userDialogs.HideLoading();
                //_userDialogs.Toast(AppResource.NoPassword);
            }
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
    }
}
