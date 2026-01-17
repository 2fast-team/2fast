using HtmlAgilityPack;

namespace Symptum.UI.Markdown.TextElements.Html;

internal class HtmlDetailsElement : IAddChild
{
    private HtmlNode _htmlNode;
    private SContainer _container = new();
    private Expander _expander;
    private FlowDocumentElement _flowDocument;

    public STextElement TextElement => _container;

    public HtmlDetailsElement(HtmlNode details, MarkdownConfiguration config)
    {
        _htmlNode = details;

        var header = _htmlNode.ChildNodes
            .FirstOrDefault(
                x => x.Name == "summary" ||
                x.Name == "header");

        _expander = new Expander
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _flowDocument = new(config, false);
        _flowDocument.StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
        _expander.Content = _flowDocument.StackPanel;
        var headerBlock = new TextBlock
        {
            Text = header?.InnerText,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _expander.Header = headerBlock;
        _container.UIElement = _expander;
    }

    public void AddChild(IAddChild child)
    {
        _flowDocument.AddChild(child);
    }
}

