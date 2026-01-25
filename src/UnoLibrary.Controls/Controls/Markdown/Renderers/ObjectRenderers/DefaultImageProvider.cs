using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

internal class DefaultImageProvider : IImageProvider
{
    private const string _uriPrefix = "ms-appx://";

    public async Task<ImageSource> GetImageSource(string url)
    {
        var image = new Image();
        var bitmap = new BitmapImage();
        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
        IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();

        // Set the source of the BitmapImage
        await bitmap.SetSourceAsync(randomStream);
        image.Source = bitmap;
        image.Width = bitmap.PixelWidth == 0 ? bitmap.DecodePixelWidth : bitmap.PixelWidth;
        image.Height = bitmap.PixelHeight == 0 ? bitmap.DecodePixelHeight : bitmap.PixelHeight;
        return image.Source;
    }

    public bool ShouldUseThisProvider(string url)
    {
        if (url.Contains(_uriPrefix))
        {
            return true;
        }
        return false;
    }
}
