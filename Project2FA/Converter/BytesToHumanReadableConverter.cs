using System;
using System.Globalization;
using WebDAVClient.Types;
using Xamarin.Forms;

namespace Project2FA.Converter
{
    public class BytesToHumanReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = decimal.Parse(value.ToString());
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            var order = 0;
            while (size >= 1024m && ++order < sizes.Length)
            {
                size = size / 1024m;
            }

            switch (order)
            {
                case 0:
                    return string.Format("{0:0} {1}", size, sizes[order]);

                case 1:
                    return string.Format("{0:0.0} {1}", size, sizes[order]);

                case 2:
                    return string.Format("{0:0.00} {1}", size, sizes[order]);
            }
            return string.Format("{0:0.000} {1}", size, sizes[order]);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new ResourceInfoModel();
        }
    }
}
