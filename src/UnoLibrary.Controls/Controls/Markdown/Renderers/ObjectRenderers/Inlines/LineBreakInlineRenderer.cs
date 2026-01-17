using Markdig.Syntax.Inlines;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class LineBreakInlineRenderer : WinUIObjectRenderer<LineBreakInline>
{
    protected override void Write(WinUIRenderer renderer, LineBreakInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.IsHard)
        {
            renderer.WriteInline(new LineBreakElement());
        }
        else
        {
            // Soft line break.
            renderer.WriteText(" ");
        }
    }
}
