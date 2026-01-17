using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class DelimiterInlineRenderer : WinUIObjectRenderer<DelimiterInline>
{
    protected override void Write(WinUIRenderer renderer, DelimiterInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        // delimiter's children are emphasized text, we don't need to explicitly render them
        // Just need to render the children of the delimiter, I think..
        //renderer.WriteText(obj.ToLiteral());
        renderer.WriteChildren(obj);
    }
}
