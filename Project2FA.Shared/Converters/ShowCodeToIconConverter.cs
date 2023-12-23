using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public class ShowCodeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\uE5F0" : "\uE5F4";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\uE5F0" : "\uE5F4";
        }
    }
}
