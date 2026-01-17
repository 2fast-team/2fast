using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class LiteralInlineRenderer : WinUIObjectRenderer<LiteralInline>
{
    protected override void Write(WinUIRenderer renderer, LiteralInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.Content.IsEmpty)
            return;

        renderer.WriteText(ref obj.Content);
    }
}
