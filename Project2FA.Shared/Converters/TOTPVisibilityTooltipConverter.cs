using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public partial class TOTPVisibilityTooltipConverter : IValueConverter
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
