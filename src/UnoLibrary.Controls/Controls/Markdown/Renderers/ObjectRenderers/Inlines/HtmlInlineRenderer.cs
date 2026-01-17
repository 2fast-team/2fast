using HtmlAgilityPack;
using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

internal class HtmlInlineRenderer : WinUIObjectRenderer<HtmlInline>
{
    protected override void Write(WinUIRenderer renderer, HtmlInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        var html = obj.Tag;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        HtmlWriter.WriteHtml(renderer, doc.DocumentNode.ChildNodes);
    }
}
