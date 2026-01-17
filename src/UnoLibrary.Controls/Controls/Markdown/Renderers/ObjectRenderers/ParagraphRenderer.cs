using Markdig.Syntax;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

public class ParagraphRenderer : WinUIObjectRenderer<ParagraphBlock>
{
    protected override void Write(WinUIRenderer renderer, ParagraphBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        ParagraphElement paragraph = new(obj, renderer.Configuration);
        // set style
        renderer.Push(paragraph);
        renderer.WriteLeafInline(obj);
        renderer.Pop();
    }
}
