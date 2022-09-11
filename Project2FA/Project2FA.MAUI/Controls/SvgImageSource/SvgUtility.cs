using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using Svg.Skia;

// based on https://github.com/muak/SvgImageSource/blob/922a0dbbbf0b175d6a5ee5766d2891ebb5b93fdf/SvgImageSource/SvgUtility.cs
namespace Project2FA.MAUI.Controls
{
    /// <summary>
    /// Svg utility.
    /// </summary>
    public static class SvgUtility
    {
        /// <summary>
        /// Creates the image.
        /// </summary>
        /// <returns>The image.</returns>
        /// <param name="stream">Stream.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        public static Task<Stream> CreateImage(Stream stream, double width, double height, Color color)
        {
            var screenScale = SvgImageSource.ScreenScale;

            var svg = new SKSvg();
            svg.Load(stream);

            var size = CalcSize(svg.Picture.CullRect.Size, width, height);
            var scale = CalcScale(svg.Picture.CullRect.Size, size, screenScale);
            var matrix = SKMatrix.CreateScale(scale.Item1, scale.Item2);

            using (var bitmap = new SKBitmap((int)(size.Width * screenScale), (int)(size.Height * screenScale)))
            using (var canvas = new SKCanvas(bitmap))
            using (var paint = new SKPaint())
            {
                if (!color.IsDefault())
                {
                    paint.ColorFilter = SKColorFilter.CreateBlendMode(ToSKColor(color), SKBlendMode.SrcIn);
                }

                canvas.Clear(SKColors.Transparent); // very very important!
                canvas.DrawPicture(svg.Picture, ref matrix, paint);

                using (var image = SKImage.FromBitmap(bitmap))
                using (var encoded = image.Encode())
                {
                    var imageStream = new MemoryStream();
                    encoded.SaveTo(imageStream);
                    imageStream.Position = 0;
                    return Task.FromResult(imageStream as Stream);
                }
            }
        }

        /// <summary>
        /// Tos the SKC olor.
        /// </summary>
        /// <returns>The SKC olor.</returns>
        /// <param name="color">Color.</param>
        public static SKColor ToSKColor(Color color)
        {
            return new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
        }

        /// <summary>
        /// Calculates the size.
        /// </summary>
        /// <returns>The size.</returns>
        /// <param name="size">Size.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public static SKSize CalcSize(SKSize size, double width, double height)
        {
            double w;
            double h;
            if (width <= 0 && height <= 0)
            {
                return size;
            }
            else if (width <= 0)
            {
                h = height;
                w = height * (size.Width / size.Height);
            }
            else if (height <= 0)
            {
                w = width;
                h = width * (size.Height / size.Width);
            }
            else
            {
                w = width;
                h = height;
            }

            return new SKSize((float)w, (float)h);
        }

        /// <summary>
        /// Calculates the scale.
        /// </summary>
        /// <returns>The scale.</returns>
        /// <param name="originalSize">Original size.</param>
        /// <param name="scaledSize">Scaled size.</param>
        /// <param name="screenScale">Screen scale.</param>
        public static Tuple<float, float> CalcScale(SKSize originalSize, SKSize scaledSize, float screenScale)
        {
            var sx = scaledSize.Width * screenScale / originalSize.Width;
            var sy = scaledSize.Height * screenScale / originalSize.Height;

            return new Tuple<float, float>((float)sx, (float)sy);
        }
    }
}
