using Markdig.Renderers;
using Markdig.Syntax;

namespace Symptum.UI.Markdown.Renderers;

public abstract class WinUIObjectRenderer<TObject> : MarkdownObjectRenderer<WinUIRenderer, TObject>
    where TObject : MarkdownObject
{
}
