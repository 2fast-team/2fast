using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Project2FA.Converter
{
    public class ProgressBarSecondConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int second = (int)value;
            double period = System.Convert.ToDouble((string)parameter);
            if (second == period)
            {
                return 0;
            }
            else if(second == 0)
            {
                return 1.0;
            }
            else
            {
                return (second / period);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
