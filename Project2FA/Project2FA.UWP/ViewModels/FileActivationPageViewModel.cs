using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.UWP.Strings;

namespace Project2FA.UWP.ViewModels
{
    public class FileActivationPageViewModel : ObservableObject
    {
        private string _password;
        public ICommand LoginCommand { get; }
        private IDialogService DialogService { get; }
        private string _applicationTitle;
        public FileActivationPageViewModel()
        {
            DialogService = App.Current.Container.Resolve<IDialogService>();
            LoginCommand = new RelayCommand(CheckLogin);
            var title = Windows.ApplicationModel.Package.Current.DisplayName;
            ApplicationTitle = System.Diagnostics.Debugger.IsAttached ? "[Debug] " + title : title;
        }

        /// <summary>
        /// Make a login with hitting 'Enter' key possible
        /// </summary>
        /// <param name="e"></param>
        public void LoginWithEnterKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                CheckLogin();
            }
        }

        /// <summary>
        /// Checks the password for the login
        /// </summary>
        private async void CheckLogin()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                if (!await CheckNavigationRequest(Password))
                {
                    Password = string.Empty;
                    await ShowLoginError();
                }
            }
        }

        /// <summary>
        /// Check the input
        /// </summary>
        /// <param name="password"></param>
        /// <returns>return true if password is valid;
        /// else decryption not working</returns>
        private async Task<bool> CheckNavigationRequest(string password)
        {
            return false;
        }

        /// <summary>
        /// Shows a wrong password error to the user
        /// </summary>
        private Task ShowLoginError()
        {
            var dialog = new ContentDialog();
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.Title = Resources.Error;
            dialog.Content = Resources.LoginPagePasswordMismatch;
            dialog.PrimaryButtonText = Resources.Confirm;
            return DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }
    }
}
