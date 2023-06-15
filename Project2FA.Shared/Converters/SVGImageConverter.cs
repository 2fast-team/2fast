using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using System.Text;
using UNOversal.Logging;
using Prism.Ioc;


#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Project2FA.UWP;
#else
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Data;
using Project2FA.UNO;
#endif

namespace Project2FA.Converters
{
    public class SVGImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var svg = new SvgImageSource();
#if __ANDROID__ || __IOS__
            svg.RasterizePixelHeight = 64;
            svg.RasterizePixelWidth = 64;
#endif
            if (value != null)
            {
                try
                {
                    //var svgBuffer = CryptographicBuffer.ConvertStringToBinary(value.ToString(), BinaryStringEncoding.Utf8);

                    //using (var stream = svgBuffer.AsStream())
                    //{
                    //    svg.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
                    //}
                    var utf8 = new UTF8Encoding();
                    var svgBuffer = utf8.GetBytes(value.ToString());

                    using (var stream = new MemoryStream(svgBuffer) { Position = 0 })
                    {
                        svg.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
                    }
                }
                catch (Exception exc)
                {
                    App.Current.Container.Resolve<ILoggerFacade>().Log(nameof(SVGImageConverter) + " " + exc.Message, Category.Exception, Priority.High);
                    return null;
                }
            }
            else
            {
                return null;
            }
            return svg;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
