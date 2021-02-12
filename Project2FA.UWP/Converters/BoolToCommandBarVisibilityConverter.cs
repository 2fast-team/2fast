using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
{
    public class BoolToCommandBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool) return (bool)value ? CommandBarOverflowButtonVisibility.Visible : CommandBarOverflowButtonVisibility.Collapsed;
            return CommandBarOverflowButtonVisibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
