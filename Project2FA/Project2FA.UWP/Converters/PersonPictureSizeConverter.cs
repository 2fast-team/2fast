using System;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
{
    public class PersonPictureSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (int.TryParse(value.ToString(), out int size))
            {
                return (size/3)*2;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
