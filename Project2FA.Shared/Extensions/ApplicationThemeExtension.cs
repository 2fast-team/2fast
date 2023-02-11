#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace Project2FA.Extensions
{
    public static class ApplicationThemeExtension
    {
        public static ElementTheme ToElementTheme(this ApplicationTheme theme)
        {
            switch (theme)
            {
                case ApplicationTheme.Light:
                    return ElementTheme.Light;
                case ApplicationTheme.Dark:
                default:
                    return ElementTheme.Dark;
            }
        }
    }
}
