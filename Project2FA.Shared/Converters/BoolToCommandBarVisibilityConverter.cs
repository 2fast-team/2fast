using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public partial class BoolToCommandBarVisibilityConverter : IValueConverter
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
