using System.Text;
using ColorCode.Uno;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class CodeBlockElement : IAddChild
{
    private CodeBlock _codeBlock;
    private SContainer _container = new();
    private MarkdownConfiguration _config;

    public STextElement TextElement => _container;

    public CodeBlockElement(CodeBlock codeBlock, MarkdownConfiguration config)
    {
        _codeBlock = codeBlock;
        _config = config;
        Border border = new()
        {
            Style = _config.Themes.CodeBlockBorderStyle
        };
        TextBlock textBlock = new()
        {
            Style = config.Themes.CodeTextBlockStyle
        };

        StringBuilder stringBuilder = new();

        if (codeBlock is FencedCodeBlock fencedCodeBlock)
        {
            var formatter = new RichTextBlockFormatter(ElementTheme.Dark);

            // go through all the lines backwards and only add the lines to a stack if we have encountered the first non-empty line
            StringLine[] lines = fencedCodeBlock.Lines.Lines;
            Stack<string> stack = new();
            bool encounteredFirstNonEmptyLine = false;
            if (lines != null)
            {
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    StringLine line = lines[i];
                    if (string.IsNullOrWhiteSpace(line.ToString()) && !encounteredFirstNonEmptyLine)
                    {
                        continue;
                    }

                    encounteredFirstNonEmptyLine = true;
                    stack.Push(line.ToString());
                }

                // append all the lines in the stack to the string builder
                while (stack.Count > 0)
                {
                    stringBuilder.AppendLine(stack.Pop());
                }
            }

            formatter.FormatInlines(stringBuilder.ToString(), fencedCodeBlock.ToLanguage(), textBlock.Inlines);
        }
        else
        {
            for (int i = 0; i < codeBlock.Lines.Lines.Length; i++)
            {
                StringLine line = codeBlock.Lines.Lines[i];
                stringBuilder.Append(line.ToString());

                if (i < codeBlock.Lines.Lines.Length - 1) stringBuilder.AppendLine();
            }
            textBlock.Text = stringBuilder.ToString();
        }
        border.Child = new ScrollViewer() { Content = textBlock, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        _container.UIElement = border;
    }

    public void AddChild(IAddChild child) { }
}
