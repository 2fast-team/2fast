using Markdig.Syntax.Inlines;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class AutoLinkInlineRenderer : WinUIObjectRenderer<AutolinkInline>
{
    protected override void Write(WinUIRenderer renderer, AutolinkInline link)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(link);

        string url = link.Url;
        if (link.IsEmail)
        {
            url = "mailto:" + url;
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            url = "#";
        }

        AutolinkInlineElement autolink = new(link);

        renderer.Push(autolink);

        renderer.WriteText(link.Url);
        renderer.Pop();
    }
}
