namespace Symptum.UI.Markdown.TextElements;

public class FlowDocumentElement : IAddChild
{
    private StackPanel _stackPanel = new();
    private SContainer _container = new();
    private MarkdownConfiguration _config;

    public STextElement TextElement => _container;

    public StackPanel StackPanel
    {
        get => _stackPanel;
        set => _stackPanel = value;
    }

    public Style? TextBlockStyle { get; set; }

    public FlowDocumentElement(MarkdownConfiguration config, bool isTopLevel = true)
    {
        _stackPanel.Spacing = config.Themes.Spacing;
        if (isTopLevel) _stackPanel.Padding = config.Themes.Padding;
        _config = config;

        _container.UIElement = _stackPanel;
    }

    public void AddChild(IAddChild child)
    {
        STextElement element = child.TextElement;
        if (element != null)
        {
            if (element is SInline inline)
            {
                TextBlock _textBlock = new()
                {
                    Style = TextBlockStyle ?? _config.Themes.BodyTextBlockStyle
                };
                _textBlock.Inlines.Add(inline.Inline);
                _stackPanel.Children.Add(_textBlock);
            }
            else if (element is SBlock block)
            {
                if (block is SParagraph paragraph)
                {
                    if (TextBlockStyle != null) paragraph.TextBlockStyle = TextBlockStyle;
                    paragraph.CreateUIElement();
                }

                if (block.UIElement is UIElement ui)
                    _stackPanel.Children.Add(ui);
            }
        }
    }
}
