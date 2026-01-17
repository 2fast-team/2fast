using HtmlAgilityPack;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using Windows.UI.Text;

namespace Symptum.UI.Markdown.TextElements.Html;

internal class HtmlInlineElement : IAddChild
{
    private HtmlNode _htmlNode;
    private Span _span = new();
    private SInline inline;

    public STextElement TextElement => inline;

    public HtmlInlineElement(HtmlNode node)
    {
        _htmlNode = node;
        inline = new()
        {
            Inline = _span
        };
        StyleInline();
    }

    public void AddChild(IAddChild child)
    {
        if (child is IAddChild textElement && textElement.TextElement is SInline _inline)
        {
            _span.Inlines.Add(_inline.Inline);
        }
        else
        {

        }
    }

    private void StyleInline()
    {
        switch (_htmlNode.Name.ToLower())
        {
            case "em":
                _span.FontStyle = FontStyle.Italic;
                break;
            case "b":
                _span.FontWeight = FontWeights.Bold;
                break;
            case "s":
                _span.TextDecorations = TextDecorations.Strikethrough;
                break;
            case "u":
                _span.TextDecorations = TextDecorations.Underline;
                break;
        }
    }
}
