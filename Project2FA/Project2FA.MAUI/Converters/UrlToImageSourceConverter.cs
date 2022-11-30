using Project2FA.MAUI.Controls;
using System.Globalization;

namespace Project2FA.MAUI.Converters
{
    public class UrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                //var uriString = (string)value;
                var uriString = "https://upload.wikimedia.org/wikipedia/commons/6/61/Uni_G%C3%B6ttingen_Siegel.svg";
                Uri uri = new Uri(uriString);
                if (uri.AbsolutePath.ToLowerInvariant().EndsWith(".svg"))
                {
                    return SvgImageSource.FromSvgUri(uriString, 68, 68, Colors.White);
                }
                else
                {
                    return ImageSource.FromUri(uri);
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
