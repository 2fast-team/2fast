using Microsoft.UI.Xaml.Media.Imaging;

namespace Symptum.UI.Markdown;

public sealed class DefaultSVGRenderer : ISVGRenderer
{
    public async Task<ImageSource> SvgToImageSource(string svgString)
    {
        SvgImageSource svgImageSource = new();
        // Create a MemoryStream object and write the SVG string to it
        using (MemoryStream memoryStream = new())
        using (StreamWriter streamWriter = new(memoryStream))
        {
            await streamWriter.WriteAsync(svgString);
            await streamWriter.FlushAsync();

            // Rewind the MemoryStream
            memoryStream.Position = 0;

            // Load the SVG from the MemoryStream
            await svgImageSource.SetSourceAsync(memoryStream.AsRandomAccessStream());
        }
        return svgImageSource;
    }
}
