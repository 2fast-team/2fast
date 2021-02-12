using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
{
    public class NullToVisibilityConverter : IValueConverter
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
