using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using Project2FA.UnoApp;

namespace Project2FA.Uno.Droid
{

    [global::Android.App.ApplicationAttribute(
        Label = "@string/ApplicationName",
        Icon = "@mipmap/schluessel",
        LargeHeap = true,
        HardwareAccelerated = true,
        Theme = "@style/AppTheme",
        RequestLegacyExternalStorage = true,
        AllowBackup =false
    )]
    public class Application : Microsoft.UI.Xaml.NativeApplication
    {
        public Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(() => new App(), javaReference, transfer)
        {
            // TODO test if logging inti is needed and working in production builds
            //App.InitializeLogging();
            ConfigureUniversalImageLoader();
        }

        private static void ConfigureUniversalImageLoader()
        {
            // Create global configuration and initialize ImageLoader with this config
            ImageLoaderConfiguration config = new ImageLoaderConfiguration
                .Builder(Context)
                .Build();

            ImageLoader.Instance.Init(config);

            ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
        }
    }
}
