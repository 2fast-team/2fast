using HtmlAgilityPack;

namespace Symptum.UI.Markdown.TextElements.Html;

internal class HtmlBlockElement : IAddChild
{
    private HtmlNode _htmlNode;
    private SParagraph _paragraph;
    private MarkdownConfiguration _config;

    public STextElement TextElement => _paragraph;

    public HtmlBlockElement(HtmlNode node, MarkdownConfiguration config)
    {
        _htmlNode = node;
        _config = config;
        var align = _htmlNode.GetAttribute("align", "left");
        _paragraph = new()
        {
            TextAlignment = align switch
            {
                "left" => TextAlignment.Left,
                "right" => TextAlignment.Right,
                "center" => TextAlignment.Center,
                "justify" => TextAlignment.Justify,
                _ => TextAlignment.Left,
            }
        };
        StyleBlock();
    }

    public void AddChild(IAddChild child) => _paragraph.AddInline(child.TextElement);

    private void StyleBlock()
    {
        switch (_htmlNode.Name.ToLower())
        {
            case "address":
                _paragraph.TextBlockStyle = _config.Themes.AddressBlockTextBlockStyle;
                break;
        }
    }
}
