using System;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
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
