using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class HtmlEntityInlineRenderer : WinUIObjectRenderer<HtmlEntityInline>
{
    protected override void Write(WinUIRenderer renderer, HtmlEntityInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        Markdig.Helpers.StringSlice transcoded = obj.Transcoded;
        renderer.WriteText(ref transcoded);
    }
}
