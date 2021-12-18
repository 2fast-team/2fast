using Project2FA.UWP.Controls;
using Project2FA.UWP.Extensions;
using Project2FA.UWP.Services;
using Project2FA.UWP.Services.Enums;
using Project2FA.UWP.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class AddAccountContentDialog : ContentDialog
    {
        //MobileBarcodeScanner _barcodeScanner;
        //MobileBarcodeScanningOptions _mobileBarcodeScanningOptions;
        public AddAccountContentDialogViewModel ViewModel => DataContext as AddAccountContentDialogViewModel;

        public AddAccountContentDialog()
        {
            this.InitializeComponent();
            //_barcodeScanner = new MobileBarcodeScanner(this.Dispatcher);
            //_barcodeScanner.RootFrame = CameraFrame;
            //_barcodeScanner.Dispatcher = this.Dispatcher;
            //_barcodeScanner.OnCameraError += _barcodeScanner_OnCameraError;
            //_barcodeScanner.OnCameraInitialized += _barcodeScanner_OnCameraInitialized;
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
        }


        private void _barcodeScanner_OnCameraInitialized()
        {
            ViewModel.IsCameraActive = true;
        }

        private void _barcodeScanner_OnCameraError(System.Collections.Generic.IEnumerable<string> errors)
        {
            ViewModel.IsCameraActive = false;
            throw new System.NotImplementedException();
        }

        public void BTN_QRCodeScan_Click(object sender, RoutedEventArgs e)
        {
            QRCodeScanTip.IsOpen = true;
        }

        private void BTN_QRCodeCameraScan_Click(object sender, RoutedEventArgs e)
        {
            //_mobileBarcodeScanningOptions = new MobileBarcodeScanningOptions
            //{
            //    UseFrontCameraIfAvailable = false
            //};
            //_barcodeScanner.Scan(_mobileBarcodeScanningOptions);
        }


        private void HLBTN_QRCodeInfo(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogQRCodeHelp,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }
    }
}
