using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public class FavouriteToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\uE735" : "\uE734";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\uE735" : "\uE734";
        }
    }
}
