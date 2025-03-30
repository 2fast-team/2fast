using CommunityToolkit.Mvvm.ComponentModel;
using OtpNet;
using Project2FA.Repository.Models;
using System;
using System.Text;
using UNOversal.Services.Dialogs;
using Windows.Storage.Streams;
using QRCoder;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Media.Imaging;
#else
using Project2FA.Uno;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class DisplayQRCodeContentDialogViewModel : ObservableObject, IDialogInitializeAsync
    {
        private string _header;
        private BitmapImage _qrImage;
        public BitmapImage QRImage { get => _qrImage; set => SetProperty(ref _qrImage, value); }
        public string Header { get => _header; set => SetProperty(ref _header, value); }

        private async Task CreateQRCode(TwoFACodeModel model)
        {
            Header = model.Label;
            StringBuilder uriBuilder = new StringBuilder("otpauth://");
            uriBuilder.Append("totp/");
            uriBuilder.Append(Uri.EscapeDataString(model.Label) + ":");
            uriBuilder.Append(Uri.EscapeDataString(model.Issuer) + "?");
            uriBuilder.Append("secret=" + Base32Encoding.ToString(model.SecretByteArray));
            uriBuilder.Append("&issuer=" + Uri.EscapeDataString(model.Label));
            uriBuilder.Append("&algorithm=" + model.HashMode.ToString().ToUpper());
            uriBuilder.Append("&digits=" + model.TotpSize);
            uriBuilder.Append("&period=" + model.Period);

            // generate QR code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(uriBuilder.ToString(), QRCodeGenerator.ECCLevel.Q,true);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);

            // create image of the QR code
            BitmapImage bitmapImage = new BitmapImage();
            using (InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream())
            {
                await randomAccessStream.WriteAsync(qrCodeAsBitmapByteArr.AsBuffer());
                randomAccessStream.Seek(0);
                await bitmapImage.SetSourceAsync(randomAccessStream);
                QRImage = bitmapImage;
            }
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("Model", out TwoFACodeModel twoFACodeModel))
            {
                await CreateQRCode(twoFACodeModel);
            }
        }
    }
}
