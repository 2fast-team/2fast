using Symptum.Markdown.Reference;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Extensions;

public class ReferenceInlineRenderer : WinUIObjectRenderer<ReferenceInline>
{
    protected override void Write(WinUIRenderer renderer, ReferenceInline obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        renderer.WriteInline(new ReferenceInlineElement(obj));
    }
}
