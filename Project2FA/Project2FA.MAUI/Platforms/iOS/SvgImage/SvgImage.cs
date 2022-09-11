using PencilKit;
using Project2FA.MAUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Project2FA.MAUI.Platforms.iOS
{
    /// <summary>
    /// Svg image.
    /// </summary>
    public static class SvgImage
    {
        /// <summary>
        /// Init this instance.
        /// </summary>
        public static void Init()
        {
            //Register(typeof(SvgImageSource), typeof(SvgImageSourceHandler));

            // gets screen's scale here. can't use MainScreen.Scale in LoadImageAsync because of not being main thread.
            SvgImageSource.ScreenScale = (float)UIScreen.MainScreen.Scale;
        }
    }
}
