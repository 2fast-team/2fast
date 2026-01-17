using Markdig.Syntax;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

public class CodeBlockRenderer : WinUIObjectRenderer<CodeBlock>
{
    protected override void Write(WinUIRenderer renderer, CodeBlock obj)
    {
        CodeBlockElement code = new(obj, renderer.Configuration);
        renderer.Push(code);
        renderer.Pop();
    }
}
