using Markdig.Syntax.Inlines;
using Microsoft.UI.Xaml.Documents;
using Windows.UI.Text;

namespace Symptum.UI.Markdown.TextElements;

public class EmphasisInlineElement : IAddChild
{
    private Span _span;
    private SInline inline;
    private EmphasisInline _markdownObject;

    private bool _isBold;
    private bool _isItalic;
    private bool _isStrikeThrough;

    public STextElement TextElement => inline;

    public EmphasisInlineElement(EmphasisInline emphasisInline)
    {
        _span = new Span();
        _markdownObject = emphasisInline;
        inline = new()
        {
            Inline = _span
        };
    }

    public void AddChild(IAddChild child)
    {
        if (child is EmphasisInlineElement emphasisInline)
        {
            if (emphasisInline._isBold) { SetBold(); }
            if (emphasisInline._isItalic) { SetItalic(); }
            if (emphasisInline._isStrikeThrough) { SetStrikeThrough(); }
            _span.Inlines.Add(emphasisInline._span);
        }
        else if (child is IAddChild textElement && textElement.TextElement is SInline _inline)
        {
            _span.Inlines.Add(_inline.Inline);
        }
        else
        {

        }
    }

    public void SetBold()
    {
        _span.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
        _isBold = true;
    }

    public void SetItalic()
    {
        _span.FontStyle = FontStyle.Italic;
        _isItalic = true;
    }

    public void SetStrikeThrough()
    {
        _span.TextDecorations = TextDecorations.Strikethrough;
        _isStrikeThrough = true;
    }

    public void SetSubscript()
    {
        _span.SetValue(Typography.VariantsProperty, FontVariants.Subscript);
    }

    public void SetSuperscript()
    {
        _span.SetValue(Typography.VariantsProperty, FontVariants.Superscript);
    }
}
