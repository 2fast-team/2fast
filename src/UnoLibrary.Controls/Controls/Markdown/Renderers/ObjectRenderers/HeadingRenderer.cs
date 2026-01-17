using Markdig.Syntax;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

public class HeadingRenderer : WinUIObjectRenderer<HeadingBlock>
{
    protected override void Write(WinUIRenderer renderer, HeadingBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        HeadingElement paragraph = new(obj, renderer.Configuration, renderer.DocumentOutline);
        renderer.Push(paragraph);
        renderer.WriteLeafInline(obj);
        renderer.Pop();
    }
}
