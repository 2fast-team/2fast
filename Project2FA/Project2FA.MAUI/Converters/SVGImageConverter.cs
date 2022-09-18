using Project2FA.MAUI.Controls;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Converters
{
    public class SVGImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource svg;
            if (value != null)
            {
                //Stream fileStream = FileSystem.Current.OpenAppPackageFileAsync("AccountIcons/" + value.ToString()).Result;
                //using StreamReader reader = new StreamReader(fileStream);
                //Stream stream = Assembly.GetManifestResourceStream("AccountIcons/" + value.ToString());
                //return SvgImageSource.FromSvgStream("AccountIcons/" + value.ToString());
                //Converting the default dotnet image to stream
                var color = Colors.Transparent;
                //var stream = new MemoryStream();
                //var writer = new StreamWriter(stream);
                //writer.Write(value.ToString());
                //writer.Flush();
                //stream.Position = 0;


                //MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(value.ToString()));
                var utf8 = new UTF8Encoding();
                var svgBuffer = utf8.GetBytes(value.ToString());


                using (Stream contentStream = new MemoryStream(svgBuffer) { Position = 0 })
                {
                    svg = ImageSource.FromStream(() => SvgUtility.CreateImage(contentStream, 68, 68, Colors.Transparent));//SvgImageSource.FromSvgStream(() => contentStream, 68,68, Colors.Transparent);
                }
                if (svg != null)
                {
                    return svg;
                }
                else
                {
                    //return svg = new ImageSource();
                }
                //StreamReader streamReader = new StreamReader(stream, Encoding.UTF8, true);

                //var imageStream = SvgUtility.CreateImage(stream, 68, 68, Colors.Transparent).Result;

                //var screenScale = SvgImageSource.ScreenScale;

                //var svg = new SKSvg();
                //svg.Load(streamReader.BaseStream);

                //var size = SvgUtility.CalcSize(svg.Picture.CullRect.Size, 68, 68);
                //var scale = SvgUtility.CalcScale(svg.Picture.CullRect.Size, size, screenScale);
                //var matrix = SKMatrix.CreateScale(scale.Item1, scale.Item2);

                //using (var bitmap = new SKBitmap((int)(size.Width * screenScale), (int)(size.Height * screenScale)))
                //using (var canvas = new SKCanvas(bitmap))
                //using (var paint = new SKPaint())
                //{
                //    if (!color.IsDefault())
                //    {
                //        paint.ColorFilter = SKColorFilter.CreateBlendMode(SvgUtility.ToSKColor(color), SKBlendMode.SrcIn);
                //    }

                //    canvas.Clear(SKColors.Transparent); // very very important!
                //    canvas.DrawPicture(svg.Picture, ref matrix, paint);

                //    using (var image = SKImage.FromBitmap(bitmap))
                //    using (var encoded = image.Encode())
                //    {
                //        var imageStream = new MemoryStream();
                //        encoded.SaveTo(imageStream);
                //        imageStream.Position = 0;
                //        return ImageSource.FromStream(() => imageStream);
                //    }
                //}

                //return ImageSource.FromStream(() => CreateImage(stream, 68, 68, Colors.Transparent));
                //if (stream != null)
                //{


                //    //using (var svg = new SKSvg())
                //    //{
                //    //    return svg.Picture.ToImage(stream, SKColors.Empty,
                //    //            SKEncodedImageFormat.Png, 100,
                //    //            1f, 1f,
                //    //            SKColorType.Rg88, SKAlphaType.Unknown,
                //    //            SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Empty, SKColorSpaceXyz.Empty));
                //    //}
                //    //return ImageSource.FromStream(() => stream);
                //}
                //else
                //{
                //    return null;
                //}
                return string.Empty;
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

        //public Stream CreateImage(Stream stream, double width, double height, Color color)
        //{
        //    var screenScale = SvgImageSource.ScreenScale;

        //    var svg = new SKSvg();
        //    svg.Load(stream);

        //    var size = SvgUtility.CalcSize(svg.Picture.CullRect.Size, 68, 68);
        //    var scale = SvgUtility.CalcScale(svg.Picture.CullRect.Size, size, screenScale);
        //    var matrix = SKMatrix.CreateScale(scale.Item1, scale.Item2);

        //    using (var bitmap = new SKBitmap((int)(size.Width * screenScale), (int)(size.Height * screenScale)))
        //    using (var canvas = new SKCanvas(bitmap))
        //    using (var paint = new SKPaint())
        //    {
        //        if (!color.IsDefault())
        //        {
        //            paint.ColorFilter = SKColorFilter.CreateBlendMode(SvgUtility.ToSKColor(color), SKBlendMode.SrcIn);
        //        }

        //        canvas.Clear(SKColors.Transparent); // very very important!
        //        canvas.DrawPicture(svg.Picture, ref matrix, paint);

        //        using (var image = SKImage.FromBitmap(bitmap))
        //        using (var encoded = image.Encode())
        //        {
        //            var imageStream = new MemoryStream();
        //            encoded.SaveTo(imageStream);
        //            imageStream.Position = 0;
        //            return imageStream;
        //        }
        //    }
        //}
    }
}
