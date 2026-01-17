namespace Symptum.UI.Markdown;

public record MarkdownConfiguration
{
    public string? BaseUrl { get; set; }

    public IImageProvider? ImageProvider { get; set; }

    public ISVGRenderer? SVGRenderer { get; set; }

    public MarkdownThemes Themes { get; set; }

    public static MarkdownConfiguration Default = new();

    public MarkdownConfiguration()
    {
        SVGRenderer = new DefaultSVGRenderer();
        Themes = MarkdownThemes.Default;
    }
}
