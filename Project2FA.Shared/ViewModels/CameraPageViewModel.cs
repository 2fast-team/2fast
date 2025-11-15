#if !WINDOWS_UWP
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using App = Project2FA.UnoApp.App;
using UNOversal.Navigation;
using Project2FA.Uno.Views;
using Project2FA.Services.Parser;
using UNOversal.Services.Dialogs;
using System.Web;
using Windows.UI.Core;
using ZXing.Net.Uno;
using CommunityToolkit.Uno.Camera.Controls;

namespace Project2FA.ViewModels
{
    [Bindable]
    public class CameraPageViewModel : ObservableRecipient, IInitialize, IConfirmNavigation
    {
        private INavigationService NavigationService { get; }
        private IProject2FAParser Project2FAParser { get; }
        private IDialogService DialogService { get; }
        private bool _foundAccount = false;
        private bool _isDetecting = true;
        public CameraBarcodeReaderControl BarcodeReaderControl { get; set;}
        public CameraPageViewModel(INavigationService navigationService, IProject2FAParser project2FAParser, IDialogService dialogService)
        {
            NavigationService = navigationService;
            Project2FAParser = project2FAParser;
            DialogService = dialogService;
        }
        public async Task ReadBarcode(BarcodeDetectionEventArgs barcodeDetectionEventArgs)
        {
            string qrcodeStr = HttpUtility.UrlDecode(barcodeDetectionEventArgs.Results.FirstOrDefault().Value);
            var valuePair = Project2FAParser.ParseQRCodeStr(qrcodeStr);
            if (valuePair.Count > 0)
            {
                if (valuePair.FirstOrDefault().Value == "totp")
                {
                    IsDetecting = false;
                    var parameter = new NavigationParameters();
                    parameter.Add("AccountValuePair", valuePair);
                    parameter.Add("QRCodeStr", qrcodeStr);
                    _foundAccount = true;
                    await App.ShellPageInstance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await NavigationService.NavigateAsync("/" + nameof(AddAccountPage), parameter);
                    });
                }
                else
                {
                    // TODO error dialog
                }
            }
            if (qrcodeStr.StartsWith("otpauth-migration://"))
            {
                IsDetecting = false;
                var parameter = new NavigationParameters();
                parameter.Add("AccountValuePair", valuePair);
                parameter.Add("QRCodeStr", qrcodeStr);
                // TODO migration page
                
            }
        }

        public void Initialize(INavigationParameters parameters)
        {
            // TabBar should not be visible
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
        }

        public bool CanNavigate(INavigationParameters parameters)
        {
            //if (!_foundAccount)
            //{
            //    Messenger.Send(new ControlDisposeMessage(true));
            //}
            DisposeCameraControl();
            return true;
        }

        public void DisposeCameraControl()
        {
            BarcodeReaderControl?.StopCameraPreview();
        }



        public bool IsDetecting
        {
            get => _isDetecting;
            set => SetProperty(ref _isDetecting, value);
        }
    }
}
#endif