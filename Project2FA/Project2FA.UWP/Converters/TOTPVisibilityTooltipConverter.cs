using System;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
{
    public class TOTPVisibilityTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(bool)value)
            {
                return Strings.Resources.AccountCodePageTooltipHideTOTP;
            }
            else
            {
                return Strings.Resources.AccountCodePageTooltipShowTOTP;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
