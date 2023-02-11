using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.UNO.Converters
{
    public class DoubleToInt32ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32((double)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
