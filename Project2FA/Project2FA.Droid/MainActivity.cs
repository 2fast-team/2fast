using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Views;
#if HAS_WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace Project2FA.Droid
{
	[Activity(
			MainLauncher = true,
			ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
			WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden
		)]
	public class MainActivity : ApplicationActivity
	{
	}
}

