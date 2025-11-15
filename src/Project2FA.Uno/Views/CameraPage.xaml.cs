using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{

	public sealed partial class CameraPage : Page
	{
		public CameraPageViewModel ViewModel => DataContext as CameraPageViewModel;
        public CameraPage()
		{
			this.InitializeComponent();
		}

        private void CameraBarcodeReaderControl_BarcodesDetected(object sender, ZXing.Net.Uno.BarcodeDetectionEventArgs e)
        {
            if (ViewModel.BarcodeReaderControl == null)
            {
                ViewModel.BarcodeReaderControl = BarcodeReaderControl;
            }
            ViewModel.ReadBarcode(e);
        }
    }
}
