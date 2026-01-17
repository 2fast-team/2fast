namespace Symptum.UI.Markdown;

public interface IImageProvider
{
    Task<ImageSource> GetImageSource(string url);

    bool ShouldUseThisProvider(string url);
}
