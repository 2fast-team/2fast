using Windows.UI.ViewManagement;
using Project2FA.Services;
using Windows.UI;
#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#else
using Project2FA.Uno;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#endif

//based on https://github.com/microsoft/Xaml-Controls-Gallery/blob/master/XamlControlsGallery/Helper/ThemeHelper.cs
namespace Project2FA.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;
        // Keep reference so it does not get optimized/garbage collected
        private static UISettings uiSettings;
        private static bool _changeTheme;

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = Window.Current;
            // Registering to color changes, thus we notice when user changes theme system wide
            uiSettings = new UISettings();
#if WINDOWS_UWP
            uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
#endif
        }

        private static void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            // Make sure we have a reference to our window so we dispatch a UI change
            if (CurrentApplicationWindow != null)
            {
                // Dispatch on UI thread so that we have a current appbar to access and change
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CurrentApplicationWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    Color lightBackground = Color.FromArgb(255, 255, 255, 255);
                    var systemColor = sender.GetColorValue(UIColorType.Background);
                    if (lightBackground == systemColor)
                    {
                        UpdateSystemTheme(ApplicationTheme.Light);
                    }
                    else
                    {
                        UpdateSystemTheme(ApplicationTheme.Dark);
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }


        public static void UpdateSystemTheme(ApplicationTheme theme)
        {
            // change only once
            if (!_changeTheme)
            {

                if (SettingsService.Instance.AppTheme == Services.Enums.Theme.System &&
                    SettingsService.Instance.OriginalAppTheme != theme)
                {
                    // invert the current theme
                    if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                    {
                        SettingsService.Instance.ResetSystemTheme(ApplicationTheme.Light);
                    }
                    else
                    {
                        SettingsService.Instance.ResetSystemTheme(ApplicationTheme.Dark);
                    }
                }
                else
                {
                    if (SettingsService.Instance.OriginalAppTheme != theme)
                    {
                        SettingsService.Instance.OriginalAppTheme = theme;
                    }
                }
                _changeTheme = true;
            }
            else
            {
                _changeTheme = false;
            }
        }
    }
}
