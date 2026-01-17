using Markdig.Syntax;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

public class ListRenderer : WinUIObjectRenderer<ListBlock>
{
    protected override void Write(WinUIRenderer renderer, ListBlock listBlock)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(listBlock);

        ListElement list = new(listBlock, renderer.Configuration, listBlock.Parent is MarkdownDocument);

        renderer.Push(list);

        foreach (Block item in listBlock)
        {
            ListItemBlock listItemBlock = (ListItemBlock)item;
            BlockContainerElement listItem = new(listItemBlock, renderer.Configuration);
            renderer.Push(listItem);
            renderer.WriteChildren(listItemBlock);
            renderer.Pop();
        }

        renderer.Pop();
    }
}
