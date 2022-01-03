using Prism.Mvvm;
using Prism.Services.Dialogs;
using Project2FA.Repository.Models;
using System;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;
using ZXing.QrCode;

namespace Project2FA.UWP.ViewModels
{
    public class DisplayQRCodeContentDialogViewModel : BindableBase, IDialogInitialize
    {
        // Property
        private WriteableBitmap _qrImage;

        public WriteableBitmap QRImage { get => _qrImage; set => _qrImage = value; }

        // Function to call
        private void CreateQRCode(TwoFACodeModel model)
        {
            StringBuilder uriBuilder = new StringBuilder("otpauth://");
            uriBuilder.Append("totp/");
            uriBuilder.Append(model.Label + ":");
            uriBuilder.Append(model.Issuer + "?");
            uriBuilder.Append("secret=" + model.SecretByteArray.ToString());
            uriBuilder.Append("&issuer=" + model.Label);
            uriBuilder.Append("&algorithm=" + model.HashMode.ToString().ToUpper());
            uriBuilder.Append("&digits=" + model.TotpSize);
            uriBuilder.Append("&period=" + model.Period);
            //DisableECI = true,
            var options = new QrCodeEncodingOptions()
            {
                CharacterSet = "UTF-8",
                Width = 250,
                Height = 250
            };

            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;
            QRImage = writer.Write(uriBuilder.ToString());
        }

        public void Initialize(IDialogParameters parameters)
        {
            throw new NotImplementedException();
        }

        
    }
}
