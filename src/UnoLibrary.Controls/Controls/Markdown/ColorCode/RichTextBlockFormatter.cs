using ColorCode.Parsing;
using ColorCode.Styling;
using Windows.UI.Text;
using ColorCode.Common;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;

namespace ColorCode.Uno;

/// <summary>
/// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
/// </summary>
public class RichTextBlockFormatter : CodeColorizerBase
{
    /// <summary>
    /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
    /// </summary>
    /// <param name="Theme">The Theme to use, determines whether to use Default Light or Default Dark.</param>
    public RichTextBlockFormatter(ElementTheme Theme, ILanguageParser languageParser = null) : this(Theme == ElementTheme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight, languageParser)
    {
    }

    /// <summary>
    /// Creates a <see cref="RichTextBlockFormatter"/>, for rendering Syntax Highlighted code to a RichTextBlock.
    /// </summary>
    /// <param name="Style">The Custom styles to Apply to the formatted Code.</param>
    /// <param name="languageParser">The language parser that the <see cref="RichTextBlockFormatter"/> instance will use for its lifetime.</param>
    public RichTextBlockFormatter(StyleDictionary Style = null, ILanguageParser languageParser = null) : base(Style, languageParser)
    {
    }

    /// <summary>
    /// Adds Syntax Highlighted Source Code to the provided RichTextBlock.
    /// </summary>
    /// <param name="sourceCode">The source code to colorize.</param>
    /// <param name="language">The language to use to colorize the source code.</param>
    /// <param name="RichText">The Control to add the Text to.</param>
    public void FormatRichTextBlock(string sourceCode, ILanguage Language, RichTextBlock RichText)
    {
        var paragraph = new Paragraph();
        RichText.Blocks.Add(paragraph);
        FormatInlines(sourceCode, Language, paragraph.Inlines);
    }

    /// <summary>
    /// Adds Syntax Highlighted Source Code to the provided InlineCollection.
    /// </summary>
    /// <param name="sourceCode">The source code to colorize.</param>
    /// <param name="language">The language to use to colorize the source code.</param>
    /// <param name="InlineCollection">InlineCollection to add the Text to.</param>
    public void FormatInlines(string sourceCode, ILanguage Language, InlineCollection InlineCollection)
    {
        this.InlineCollection = InlineCollection;
        languageParser.Parse(sourceCode, Language, (parsedSourceCode, captures) => Write(parsedSourceCode, captures));
    }

    private InlineCollection InlineCollection { get; set; }

    protected override void Write(string parsedSourceCode, IList<Scope> scopes)
    {
        var styleInsertions = new List<TextInsertion>();

        foreach (Scope scope in scopes)
            GetStyleInsertionsForCapturedStyle(scope, styleInsertions);

        styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

        int offset = 0;

        Scope PreviousScope = null;

        foreach (var styleinsertion in styleInsertions)
        {
            var text = parsedSourceCode.Substring(offset, styleinsertion.Index - offset);
            CreateSpan(text, PreviousScope);
            if (!string.IsNullOrWhiteSpace(styleinsertion.Text))
            {
                CreateSpan(text, PreviousScope);
            }
            offset = styleinsertion.Index;

            PreviousScope = styleinsertion.Scope;
        }

        var remaining = parsedSourceCode.Substring(offset);
        // Ensures that those loose carriages don't run away!
        if (remaining != "\r")
        {
            CreateSpan(remaining, null);
        }
    }

    private void CreateSpan(string Text, Scope scope)
    {
        var span = new Span();
        var run = new Run
        {
            Text = Text
        };

        // Styles and writes the text to the span.
        if (scope != null) StyleRun(run, scope);
        span.Inlines.Add(run);

        InlineCollection.Add(span);
    }

    private void StyleRun(Run Run, Scope Scope)
    {
        string? foreground = null;
        string? background = null;
        bool italic = false;
        bool bold = false;

        if (Styles.Contains(Scope.Name))
        {
            Styling.Style style = Styles[Scope.Name];

            foreground = style.Foreground;
            background = style.Background;
            italic = style.Italic;
            bold = style.Bold;
        }

        if (!string.IsNullOrWhiteSpace(foreground))
            Run.Foreground = foreground.GetSolidColorBrush();

        //Background isn't supported, but a workaround could be created.

        if (italic)
            Run.FontStyle = FontStyle.Italic;

        if (bold)
            Run.FontWeight = FontWeights.Bold;
    }

    private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
    {
        styleInsertions.Add(new TextInsertion
        {
            Index = scope.Index,
            Scope = scope
        });

        foreach (Scope childScope in scope.Children)
            GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

        styleInsertions.Add(new TextInsertion
        {
            Index = scope.Index + scope.Length
        });
    }
}

public static class ExtensionMethods
{
    public static SolidColorBrush GetSolidColorBrush(this string hex)
    {
        hex = hex.Replace("#", string.Empty);

        byte a = 255;
        int index = 0;

        if (hex.Length == 8)
        {
            a = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
            index += 2;
        }

        byte r = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        index += 2;
        byte g = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        index += 2;
        byte b = (byte)(Convert.ToUInt32(hex.Substring(index, 2), 16));
        SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        return myBrush;
    }
}
