using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Project2FA.Converter
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dt = value as DateTime?;
            return dt != null ? dt.Value.ToString(CultureInfo.CurrentCulture) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString().Trim().Length == 0)
            {
                return null;
            }
            var s = value.ToString();

            if (string.Compare(s, "today", StringComparison.Ordinal) == 0)
            {
                return DateTime.Today;
            }
            if (string.Compare(s, "now", StringComparison.Ordinal) == 0)
            {
                return DateTime.Now;
            }
            if (string.Compare(s, "yesterday", StringComparison.Ordinal) == 0)
            {
                return DateTime.Today.AddDays(-1);
            }
            if (string.Compare(s, "tomorrow", StringComparison.Ordinal) == 0)
            {
                return DateTime.Today.AddDays(1);
            }

            DateTime dt;
            return DateTime.TryParse(value.ToString(), out dt);
        }
    }
}
