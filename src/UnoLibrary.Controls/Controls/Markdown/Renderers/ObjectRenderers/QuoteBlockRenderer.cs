using Markdig.Extensions.Alerts;
using Markdig.Syntax;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

public class QuoteBlockRenderer : WinUIObjectRenderer<QuoteBlock>
{
    protected override void Write(WinUIRenderer renderer, QuoteBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        QuoteElement quote = new(obj, renderer.Configuration, (obj as AlertBlock)?.Kind);

        renderer.Push(quote);
        renderer.WriteChildren(obj);
        renderer.Pop();
    }
}
