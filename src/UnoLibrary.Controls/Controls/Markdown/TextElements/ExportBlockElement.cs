using Symptum.Markdown.Embedding;

namespace Symptum.UI.Markdown.TextElements;

public class ExportBlockElement : IAddChild
{
    private ExportBlock _exportBlock;
    private SContainer _container = new();
    private FlowDocumentElement _flowDocument;

    public STextElement TextElement => _container;

    public ExportBlockElement(ExportBlock exportBlock, MarkdownConfiguration config)
    {
        _exportBlock = exportBlock;
        _flowDocument = new FlowDocumentElement(config, false);
        _container.UIElement = _flowDocument.StackPanel;
    }

    public void AddChild(IAddChild child)
    {
        _flowDocument.AddChild(child);
    }
}
