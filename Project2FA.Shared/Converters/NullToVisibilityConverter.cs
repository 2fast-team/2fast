using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public partial class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var invert = parameter != null;
            var str = value as string;
            if (str != null)
            {
                return string.IsNullOrEmpty(str)
                    ? (invert ? Visibility.Visible : Visibility.Collapsed)
                    : (invert ? Visibility.Collapsed : Visibility.Visible);
            }
            return value == null
                ? (invert ? Visibility.Visible : Visibility.Collapsed)
                : (invert ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
