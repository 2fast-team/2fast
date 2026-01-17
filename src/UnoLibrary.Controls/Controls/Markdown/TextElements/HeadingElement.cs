using HtmlAgilityPack;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class HeadingElement : IAddChild
{
    private SParagraph _paragraph = new();
    private HeadingBlock? _headingBlock;
    private HtmlNode? _htmlNode;
    private MarkdownConfiguration? _config;

    public STextElement TextElement => _paragraph;

    public HeadingElement(HeadingBlock headingBlock, MarkdownConfiguration config, DocumentOutline outline)
    {
        _headingBlock = headingBlock;
        LoadHeadingElement(config, outline, headingBlock.Level,
            headingBlock.GetAttributes().Id, headingBlock.Inline?.FirstChild?.ToString());
    }

    public HeadingElement(HtmlNode htmlNode, MarkdownConfiguration config, DocumentOutline outline)
    {
        _htmlNode = htmlNode;
        var align = _htmlNode.GetAttribute("align", "left");
        _paragraph.TextAlignment = align switch
        {
            "left" => TextAlignment.Left,
            "right" => TextAlignment.Right,
            "center" => TextAlignment.Center,
            "justify" => TextAlignment.Justify,
            _ => TextAlignment.Left,
        };

        if (int.TryParse(htmlNode.Name.AsSpan(1), out int level))
            LoadHeadingElement(config, outline, level, htmlNode.Id, htmlNode.InnerText);
    }

    private void LoadHeadingElement(MarkdownConfiguration config, DocumentOutline outline, int level, string? id, string? title)
    {
        _config = config;
        _paragraph.TextBlockStyle = level switch
        {
            1 => _config.Themes.H1TextBlockStyle,
            2 => _config.Themes.H2TextBlockStyle,
            3 => _config.Themes.H3TextBlockStyle,
            4 => _config.Themes.H4TextBlockStyle,
            5 => _config.Themes.H5TextBlockStyle,
            _ => _config.Themes.H6TextBlockStyle,
        };

        DocumentNode node = new()
        {
            Id = id,
            Level = level switch
            {
                1 => DocumentLevel.Heading1,
                2 => DocumentLevel.Heading2,
                3 => DocumentLevel.Heading3,
                4 => DocumentLevel.Heading4,
                5 => DocumentLevel.Heading5,
                _ => DocumentLevel.Heading6,
            },
            Navigate = OnNavigate,
            Title = title
        };

        outline.PushNode(node);
    }

    public void OnNavigate()
    {
        _paragraph.UIElement?.StartBringIntoView(new() { HorizontalAlignmentRatio = 0, VerticalAlignmentRatio = 0 });
    }

    public void AddChild(IAddChild child)
    {
        _paragraph.AddInline(child.TextElement);
    }
}
