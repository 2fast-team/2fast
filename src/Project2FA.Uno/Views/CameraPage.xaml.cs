using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{

	public sealed partial class CameraPage : Page
	{
		public CameraPageViewModel ViewModel => DataContext as CameraPageViewModel;
        public CameraPage()
		{
			this.InitializeComponent();
            this.Loaded += CameraPage_Loaded;
		}

        private void CameraPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.BarcodeReaderControl = BarcodeReaderControl;
        }

        private void CameraBarcodeReaderControl_BarcodesDetected(object sender, ZXing.Net.Uno.BarcodeDetectionEventArgs e)
        {
            ViewModel.ReadBarcode(e);
        }
    }
}
