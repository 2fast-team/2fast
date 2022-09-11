using Foundation;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Project2FA.MAUI.Controls;
using UIKit;

// based on https://github.com/muak/SvgImageSource/blob/922a0dbbbf0b175d6a5ee5766d2891ebb5b93fdf/SvgImageSource.iOS/SvgImageSourceHandler.cs
namespace Project2FA.MAUI.Platforms.iOS
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
        /// <param name="cancelationToken">Cancelation token.</param>
        /// <param name="scale">Scale.</param>
        public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
        {
            var svgImageSource = imagesource as SvgImageSource;

            using (var stream = await svgImageSource.GetImageStreamAsync(cancelationToken).ConfigureAwait(false))
            {
                if (stream == null)
                {
                    return null;
                }
                return UIImage.LoadFromData(NSData.FromStream(stream), SvgImageSource.ScreenScale);
            }
        }

    }
}
