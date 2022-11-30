using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Project2FA.MAUI.Helpers
{
    internal class LoadImageDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
#if IOS || ANDROID || MACCATALYST
            IImage image;
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("Project2FA.MAUI.Resources.Images.dotnet_bot.png"))
            {
                // PlatformImage isn't currently supported on Windows.
                image = PlatformImage.FromStream(stream);
            }

            if (image != null)
            {
                canvas.DrawImage(image, 10, 10, 68, 68);
            }
#endif
        }
    }
}
