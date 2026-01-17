using Symptum.Markdown.Embedding;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Extensions;

public class ImportBlockRenderer : WinUIObjectRenderer<ImportBlock>
{
    protected override void Write(WinUIRenderer renderer, ImportBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        renderer.Push(new ImportBlockElement(obj, renderer));
        renderer.Pop();
    }
}
