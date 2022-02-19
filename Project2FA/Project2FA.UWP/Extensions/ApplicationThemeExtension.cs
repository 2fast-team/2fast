using Windows.UI.Xaml;

namespace Project2FA.UWP.Extensions
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
