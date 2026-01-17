using Markdig;
using Markdig.Syntax;
using Symptum.Markdown.Embedding;
using Symptum.Markdown.Reference;
using Symptum.UI.Markdown.Renderers;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown;

[TemplatePart(Name = MarkdownContainerName, Type = typeof(Grid))]
public partial class MarkdownTextBlock : Control
{
    private const string MarkdownContainerName = "MarkdownContainer";
    private Grid? _container;
    internal MarkdownPipeline _pipeline;
    private FlowDocumentElement _document;
    private WinUIRenderer? _renderer;

    public MarkdownTextBlock()
    {
        DefaultStyleKey = typeof(MarkdownTextBlock);
        _document = new FlowDocumentElement(Configuration);
        _pipeline = new MarkdownPipelineBuilder()
            .UseAlertBlocks()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .UseListExtras()
            .UseTaskLists()
            .UsePipeTables()
            .UseGridTables()
            .UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub)
            .Use<ReferenceInlineExtension>()
            .Use<ExportBlockExtension>()
            .Use<ImportBlockExtension>()
            .Build();
        DocumentOutline = new();
        ImportsHandler = new();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = (Grid)GetTemplateChild(MarkdownContainerName);
        _container.Children.Clear();
        _container.Children.Add(_document.StackPanel);
        Build();
    }

    private void ApplyConfig(MarkdownConfiguration config)
    {
        if (_renderer != null)
        {
            _renderer.Configuration = config;
        }
    }

    private void ApplyText(bool rerender)
    {
        if (_renderer != null)
        {
            if (rerender)
            {
                _renderer.ReloadDocument();
            }

            MarkdownDocument? markdown = null;
            try
            {
                if (!string.IsNullOrEmpty(Text))
                    markdown = Markdig.Markdown.Parse(Text, _pipeline);
            }
            catch { }

            if (markdown != null)
            {
                MarkdownDocument = markdown;
                MarkdownParsed?.Invoke(this, new(markdown));
                _renderer.Render(markdown);
                MarkdownRendered?.Invoke(this, null);
            }
        }
        else _document.StackPanel.Children.Clear();
    }

    private void Build()
    {
        if (Configuration != null)
        {
            if (_renderer == null)
            {
                _renderer = new WinUIRenderer(this, _document);
            }
            _pipeline.Setup(_renderer);
            ApplyText(false);
        }
    }

    public event EventHandler<MarkdownParsedEventArgs>? MarkdownParsed;

    public event EventHandler? MarkdownRendered;
}
