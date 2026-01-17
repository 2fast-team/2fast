using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class BlockContainerElement : IAddChild
{
    private ContainerBlock _containerBlock;
    private SContainer _container = new();
    private FlowDocumentElement _flowDocument;

    public STextElement TextElement => _container;

    public BlockContainerElement(ContainerBlock containerBlock, MarkdownConfiguration config)
    {
        _containerBlock = containerBlock;
        _flowDocument = new FlowDocumentElement(config, false);
        _container.UIElement = _flowDocument.StackPanel;
    }

    public void AddChild(IAddChild child)
    {
        _flowDocument.AddChild(child);
    }
}
