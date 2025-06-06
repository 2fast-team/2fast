using CommunityToolkit.Mvvm.ComponentModel;
using OtpNet;
using Project2FA.Repository.Models;
using System;
using System.Text;
using UNOversal.Services.Dialogs;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Project2FA.Repository.Models.Enums;

#if WINDOWS_UWP
using QRCoder;
using static QRCoder.PayloadGenerator;
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

        private string _url;

        public string Url { get => _url; set => SetProperty(ref _url, value); }
        public string Header { get => _header; set => SetProperty(ref _header, value); }


#if WINDOWS_UWP
        private async Task CreateQRCode(TwoFACodeModel model)
#else
        private void CreateQRCode(TwoFACodeModel model)
#endif
        {
            Header = model.Label;
            StringBuilder uriBuilder = new StringBuilder("otpauth://");
            if (model.OTPType == OTPType.steam.ToString())
            {
                uriBuilder.Append("steam://");
            }
            else
            {
                uriBuilder.Append("totp/");
            }
            uriBuilder.Append(Uri.EscapeDataString(model.Label) + ":");
            uriBuilder.Append(Uri.EscapeDataString(model.Issuer) + "?");
            uriBuilder.Append("secret=" + Base32Encoding.ToString(model.SecretByteArray));
            uriBuilder.Append("&issuer=" + Uri.EscapeDataString(model.Label));
            uriBuilder.Append("&algorithm=" + model.HashMode.ToString().ToUpper());
            uriBuilder.Append("&digits=" + model.TotpSize);
            uriBuilder.Append("&period=" + model.Period);

            Url = uriBuilder.ToString();

#if WINDOWS_UWP
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
#endif
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<TwoFACodeModel>("Model", out TwoFACodeModel twoFACodeModel))
            {
#if WINDOWS_UWP
                await CreateQRCode(twoFACodeModel);
#else
                CreateQRCode(twoFACodeModel);
#endif
            }
        }
    }
}
