using Markdig.Helpers;
using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class QuoteElement : IAddChild
{
    private SContainer _container;
    private FlowDocumentElement _flowDocument;
    private QuoteBlock _quoteBlock;

    public STextElement TextElement => _container;

    public QuoteElement(QuoteBlock quoteBlock, MarkdownConfiguration config, StringSlice? kind = null)
    {
        _quoteBlock = quoteBlock;
        _container = new();

        _flowDocument = new FlowDocumentElement(config, false);
        AlertKind alertKind = AlertKind.None;

        if (kind != null && kind?.Length < 16)
        {
            Span<char> upperKind = stackalloc char[kind?.Length ?? 0];
            kind?.AsSpan().ToUpperInvariant(upperKind);
            alertKind = upperKind switch
            {
                "NOTE" => AlertKind.Note,
                "TIP" => AlertKind.Tip,
                "IMPORTANT" => AlertKind.Important,
                "WARNING" => AlertKind.Warning,
                "CAUTION" => AlertKind.Caution,
                _ => AlertKind.None
            };
        }

        QuoteControl quote = new()
        {
            Kind = alertKind,
            Content = _flowDocument.StackPanel
        };

        _container.UIElement = quote;

        quote.Style = alertKind switch
        {
            AlertKind.Note => config.Themes.NoteQuoteControlStyle,
            AlertKind.Tip => config.Themes.TipQuoteControlStyle,
            AlertKind.Important => config.Themes.ImportantQuoteControlStyle,
            AlertKind.Warning => config.Themes.WarningQuoteControlStyle,
            AlertKind.Caution => config.Themes.CautionQuoteControlStyle,
            _ => config.Themes.DefaultQuoteControlStyle
        };
    }

    public void AddChild(IAddChild child)
    {
        _flowDocument.AddChild(child);
    }
}
