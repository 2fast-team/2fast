using System;
using Android.Content;
using Project2FA.MAUI.Controls;


// based on https://github.com/muak/SvgImageSource/blob/922a0dbbbf0b175d6a5ee5766d2891ebb5b93fdf/SvgImageSource.Droid/SvgImage.cs
namespace Project2FA.MAUI.Platforms.Android
{

    /// <summary>
    /// Svg image.
    /// </summary>
    public static class SvgImage
    {
        /// <summary>
        /// Init this instance.
        /// </summary>
        public static void Init(Context context)
        {
            //Registered.Register(typeof(SvgImageSource), typeof(SvgImageSourceHandler));

            using (var display = context.Resources.DisplayMetrics)
            {
                SvgImageSource.ScreenScale = display.Density;
            }
        }
    }
}
