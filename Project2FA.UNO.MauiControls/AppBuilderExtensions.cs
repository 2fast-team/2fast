using Microsoft.Maui.Hosting;
using ZXing.Net.Maui.Controls;

namespace Project2FA.UNO.MauiControls;

public static class AppBuilderExtensions
{
    public static MauiAppBuilder UseMauiControls(this MauiAppBuilder builder) =>
        builder
            .UseBarcodeReader();
}
