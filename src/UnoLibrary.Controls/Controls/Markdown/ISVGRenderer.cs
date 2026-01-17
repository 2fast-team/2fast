namespace Symptum.UI.Markdown;

public interface ISVGRenderer
{
    Task<ImageSource> SvgToImageSource(string svgString);
}
