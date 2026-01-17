using Markdig.Syntax.Inlines;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class CodeInlineRenderer : WinUIObjectRenderer<CodeInline>
{
    protected override void Write(WinUIRenderer renderer, CodeInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        renderer.WriteInline(new CodeInlineElement(obj, renderer.Configuration));
    }
}
