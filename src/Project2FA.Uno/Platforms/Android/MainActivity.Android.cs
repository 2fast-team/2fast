using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Windows.Storage;
using Project2FA.Services;

namespace Project2FA.Uno.Droid
{
    //open the .2fa files via the app
    [IntentFilter(new[] { Intent.ActionView, Intent.ActionEdit },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataHost = "*",
    DataMimeType = "application/octet-stream",
    DataPathPattern = ".*\\.2fa")]
    [Activity(
            MainLauncher = true,
            ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
            WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
        )]
    public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
    {
        public static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

            base.OnCreate(savedInstanceState);

            Instance = this;
            // disable screenshot capture
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            }
            Intent intent = this.Intent;
            var action = intent.Action;
            var strLink = intent.Data;
            if (Intent.ActionView == action && !string.IsNullOrWhiteSpace(strLink.ToString()))
            {
                DataService.Instance.ActivatedDatafile = StorageFile.GetFromSafUri(strLink);
            }
        }


    }
}

