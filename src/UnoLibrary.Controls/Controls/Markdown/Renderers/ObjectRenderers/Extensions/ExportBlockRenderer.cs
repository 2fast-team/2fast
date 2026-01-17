using Symptum.Markdown.Embedding;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Extensions;

public class ExportBlockRenderer : WinUIObjectRenderer<ExportBlock>
{
    protected override void Write(WinUIRenderer renderer, ExportBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        renderer.Push(new ExportBlockElement(obj, renderer.Configuration));
        renderer.WriteChildren(obj);
        renderer.Pop();
    }
}
