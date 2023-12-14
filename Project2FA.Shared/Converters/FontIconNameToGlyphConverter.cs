using Project2FA.Services;
using System;
using System.Linq;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public class FontIconNameToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string name)
            {
                var model = DataService.Instance.FontIconCollection.Where(x => x.Name == name).FirstOrDefault();
                if (model != null)
                {
                    return ((char)model.UnicodeIndex).ToString();
                }
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
