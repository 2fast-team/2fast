using CommunityToolkit.Mvvm.ComponentModel;
using OtpNet;
using Prism.Mvvm;
using Project2FA.Repository.Models;
using System;
using System.Text;
using System.Web;
using UNOversal.Services.Dialogs;
using ZXing;
using ZXing.QrCode;
#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Media.Imaging;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class DisplayQRCodeContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private string _header;
        private WriteableBitmap _qrImage;
        public WriteableBitmap QRImage { get => _qrImage; set => SetProperty(ref _qrImage, value); }
        public string Header { get => _header; set => SetProperty(ref _header, value); }

        private void CreateQRCode(TwoFACodeModel model)
        {
            Header = model.Label;
            StringBuilder uriBuilder = new StringBuilder("otpauth://");
            uriBuilder.Append("totp/");
            uriBuilder.Append(model.Label + ":");
            uriBuilder.Append(model.Issuer + "?");
            uriBuilder.Append("secret=" + Base32Encoding.ToString(model.SecretByteArray));
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

            Project2FA.ZXing.BarcodeWriter writer = new Project2FA.ZXing.BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;
            QRImage = writer.Write(uriBuilder.ToString());
        }

        public void Initialize(IDialogParameters parameters)
        {
            if(parameters.TryGetValue<TwoFACodeModel>("Model", out TwoFACodeModel twoFACodeModel))
            {
                CreateQRCode(twoFACodeModel);
            }
        }
    }
}
