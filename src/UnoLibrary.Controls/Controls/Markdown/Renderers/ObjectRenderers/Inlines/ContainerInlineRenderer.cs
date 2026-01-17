
using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Inlines;

public class ContainerInlineRenderer : WinUIObjectRenderer<ContainerInline>
{
    protected override void Write(WinUIRenderer renderer, ContainerInline obj)
    {
        foreach (Inline inline in obj)
        {
            renderer.Write(inline);
        }
    }
}
