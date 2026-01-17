using Symptum.Markdown.Embedding;
using Symptum.UI.Markdown.Renderers;

namespace Symptum.UI.Markdown.TextElements;

public class ImportBlockElement : IAddChild
{
    private ImportBlock _importBlock;
    private SContainer _container = new();
    private FlowDocumentElement _flowDocument;
    private WinUIRenderer _renderer;
    private string _importId;

    public STextElement TextElement => _container;

    public ImportBlockElement(ImportBlock importBlock, WinUIRenderer renderer)
    {
        _importBlock = importBlock;
        _importId = importBlock.Id.ToString();
        _flowDocument = new FlowDocumentElement(renderer.Configuration, false);
        _container.UIElement = _flowDocument.StackPanel;
        _renderer = renderer;
        _renderer.ImportsHandler.RegisterForImport(_importId, this);
    }

    public void AddChild(IAddChild child)
    {
        _flowDocument.AddChild(child);
    }
}
