using Android.Content;
using Android.Graphics;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Project2FA.MAUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// based on https://github.com/muak/SvgImageSource/blob/922a0dbbbf0b175d6a5ee5766d2891ebb5b93fdf/SvgImageSource.Droid/SvgImageSourceHandler.cs
namespace Project2FA.MAUI.Platforms.Android
{
    /// <summary>
    /// Svg image source handler.
    /// </summary>
    public class SvgImageSourceHandler : IImageSourceHandler
    {
        /// <summary>
        /// Loads the image async.
        /// </summary>
        /// <returns>The image async.</returns>
        /// <param name="imagesource">Imagesource.</param>
        /// <param name="context">Context.</param>
        /// <param name="cancelationToken">Cancelation token.</param>
        public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
        {
            var svgImageSource = imagesource as SvgImageSource;

            using (var stream = await svgImageSource.GetImageStreamAsync(cancelationToken).ConfigureAwait(false))
            {
                if (stream == null)
                {
                    return null;
                }
                return await BitmapFactory.DecodeStreamAsync(stream);
            }
        }
    }
}
