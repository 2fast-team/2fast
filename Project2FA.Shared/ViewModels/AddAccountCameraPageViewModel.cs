#if !WINDOWS_UWP
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO.MauiControls;
using CommunityToolkit.Mvvm.Messaging;
using App = Project2FA.UNO.App;
using UNOversal.Navigation;
using Project2FA.UNO.Views;
using Project2FA.Services.Parser;
using System.Linq;
using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using System.Web;

namespace Project2FA.ViewModels
{
    [Bindable]
    public class AddAccountCameraPageViewModel : ObservableRecipient, IInitialize, IConfirmNavigationAsync
    {
        private INavigationService NavigationService { get; }
        private IProject2FAParser Project2FAParser { get; }
        private IDialogService DialogService { get; }
        private bool _finishAccount = false;
        public AddAccountCameraPageViewModel(INavigationService navigationService, IProject2FAParser project2FAParser, IDialogService dialogService)
        {
            NavigationService = navigationService;
            Project2FAParser = project2FAParser;
            DialogService = dialogService;

            Messenger.Register<AddAccountCameraPageViewModel, QRCodeScannedMessage>(this, async (viewmodel, message) =>
            {
                List<KeyValuePair<string, string>> valuePair = Project2FAParser.ParseQRCodeStr(HttpUtility.UrlDecode(message.Value));
                if (valuePair.FirstOrDefault().Value == "totp")
                {
                    var parameter = new NavigationParameters();
                    parameter.Add("AccountValuePair", valuePair);
                    await NavigationService.NavigateAsync(nameof(AddAccountPage), parameter);
                }
                else
                {
                    // TODO error dialog
                }
            });
        }

        public void Initialize(INavigationParameters parameters)
        {
            // TabBar should not be visible
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;



        }

        public Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            if (!_finishAccount)
            {
                // TODO Dialog 
                return Task.FromResult(true);
            }
            else
            {
                //Messenger.Unregister<QRCodeScannedMessage>(this);
                return Task.FromResult(true);
            }
        }
    }
}
#endif