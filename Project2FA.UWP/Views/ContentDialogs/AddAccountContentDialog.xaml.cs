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
        long _tagToken;

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
            _tagToken = TB_AddAccountContentDialogSecretKey.RegisterPropertyChangedCallback(TagProperty, tbTagChangedCallback);
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

        private void tbTagChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == TextBlock.TagProperty)
            {
                if (((TextBox)sender).Tag is string tag)
                {
                    if (tag == "ValidationError")
                    {
                        AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
                        {
                            Target = TB_AddAccountContentDialogSecretKey as FrameworkElement,
                            Subtitle = Strings.Resources.AddAccountCodeContentDialogInputSecretKeyHelp,
                            AutoCloseInterval = 3000,
                            IsLightDismissEnabled = false,
                            BorderThickness = new Thickness(2,2,2,2),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(255,255, 28,32)),
                            IsOpen = true,
                        };
                        RootGrid.Children.Add(teachingTip);
                    }
                }
            }
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

        private void HLBTN_SecretKeyInfo(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogInputSecretKeyHelp,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }
    }
}
